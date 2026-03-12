/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Application.Mapping;
using GestionProduccion.Application.Mappers;
using System.Security.Cryptography;
using System.Text;

namespace GestionProduccion.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IProductionOrderRepository _orderRepository;
    private readonly IPasswordResetTokenRepository _passwordResetRepo;
    private readonly IUserRefreshTokenRepository _refreshTokenRepo;
    private readonly ISewingTeamRepository _teamRepository;
    private readonly MainMapper _mapper;

    public UserService(
        IUserRepository userRepository,
        IProductionOrderRepository orderRepository,
        IPasswordResetTokenRepository passwordResetRepo,
        IUserRefreshTokenRepository refreshTokenRepo,
        ISewingTeamRepository teamRepository,
        MainMapper mapper)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
        _passwordResetRepo = passwordResetRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _teamRepository = teamRepository;
        _mapper = mapper;
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return (user != null && user.IsActive) ? _mapper.ToDto(user) : null;
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        }

        var user = await _userRepository.GetByEmailAsync(email);
        return (user != null && user.IsActive) ? _mapper.ToDto(user) : null;
    }

    public async Task<List<UserDto>> GetActiveUsersAsync()
    {
        var entities = await _userRepository.GetAllActiveAsync();
        return _mapper.ToDtoList(entities);
    }

    public async Task<List<UserDto>> GetUsersByRoleAsync(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role cannot be empty.", nameof(role));
        }

        var entities = await _userRepository.GetByRoleAsync(role);
        return _mapper.ToDtoList(entities);
    }

    public async Task<bool> IsUserAssignedToOrderAsync(int userId, int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null || order.UserId != userId || order.AssignedUser == null || !order.AssignedUser.IsActive)
        {
            return false;
        }
        return true;
    }

    public async Task<List<ProductionOrderDto>> GetUserAssignedOrdersAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var orders = await _orderRepository.GetAssignedToUserAsync(userId);
        return _mapper.ToDtoList(orders);
    }

    public async Task<UserDto> CreateUserAsync(UserDto userDto, string password)
    {
        if (userDto == null) throw new ArgumentNullException(nameof(userDto));
        if (string.IsNullOrWhiteSpace(userDto.FullName)) throw new InvalidOperationException("User name cannot be empty.");
        if (string.IsNullOrWhiteSpace(userDto.Email)) throw new InvalidOperationException("User email cannot be empty.");
        if (string.IsNullOrWhiteSpace(password)) throw new InvalidOperationException("Password cannot be empty.");

        if (userDto.SewingTeamId.HasValue)
        {
            var team = await _teamRepository.GetByIdAsync(userDto.SewingTeamId.Value);
            if (team == null) throw new InvalidOperationException($"Sewing Team with ID {userDto.SewingTeamId} not found.");
        }

        var existingUser = await _userRepository.GetByEmailAsync(userDto.Email);
        if (existingUser != null)
        {
            if (existingUser.IsActive)
            {
                throw new InvalidOperationException($"O usuário com o e-mail '{userDto.Email}' já existe e está activo.");
            }
            else
            {
                existingUser.FullName = userDto.FullName;
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                existingUser.Role = userDto.Role;
                existingUser.SewingTeamId = userDto.SewingTeamId;
                existingUser.IsActive = true;
                
                await _userRepository.UpdateAsync(existingUser);
                await _userRepository.SaveChangesAsync();
                return _mapper.ToDto(existingUser);
            }
        }

        var user = userDto.ToEntity();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        user.IsActive = true;
        
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();
        return _mapper.ToDto(user);
    }

    public async Task<bool> UpdateUserAvatarAsync(int userId, string avatarUrl)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.AvatarUrl = avatarUrl;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<UserDto> UpdateUserAsync(UserDto userDto)
    {
        if (userDto == null) throw new ArgumentNullException(nameof(userDto));

        var existingUser = await _userRepository.GetByIdAsync(userDto.Id);
        if (existingUser == null) throw new KeyNotFoundException("User not found.");

        if (userDto.SewingTeamId.HasValue)
        {
            var team = await _teamRepository.GetByIdAsync(userDto.SewingTeamId.Value);
            if (team == null) throw new InvalidOperationException($"Sewing Team with ID {userDto.SewingTeamId} not found.");
        }

        existingUser.FullName = userDto.FullName;
        existingUser.Email = userDto.Email;
        existingUser.Role = userDto.Role;
        existingUser.AvatarUrl = userDto.AvatarUrl;
        existingUser.IsActive = userDto.IsActive;
        existingUser.SewingTeamId = userDto.SewingTeamId;

        await _userRepository.UpdateAsync(existingUser);
        await _userRepository.SaveChangesAsync();
        return _mapper.ToDto(existingUser);
    }

    public async Task<UserDto> UpdateProfileAsync(int userId, string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new InvalidOperationException("Name cannot be empty.");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        user.FullName = fullName;
        
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        return _mapper.ToDto(user);
    }

    public async Task<bool> DeactivateUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        user.IsActive = false;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash)) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<string?> RequestPasswordResetAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || !user.IsActive) return null;

        var token = GenerateSecureToken();
        var tokenHash = ComputeHash(token);

        await _passwordResetRepo.AddAsync(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiryDate = DateTime.UtcNow.AddHours(1),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        });

        return token;
    }

    public async Task<bool> CompletePasswordResetAsync(string email, string token, string newPassword)
    {
        var tokenHash = ComputeHash(token);
        var resetToken = await _passwordResetRepo.GetByHashAsync(tokenHash);

        if (resetToken == null || resetToken.IsUsed || resetToken.ExpiryDate <= DateTime.UtcNow) return false;
        if (resetToken.User.Email.ToLower() != email.ToLower()) return false;

        var user = resetToken.User;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _userRepository.UpdateAsync(user);

        resetToken.IsUsed = true;
        await _passwordResetRepo.UpdateAsync(resetToken);
        await _refreshTokenRepo.RevokeAllUserTokensAsync(user.Id);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<UserDto?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || !user.IsActive) return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

        return user.ToDto();
    }

    private string GenerateSecureToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }

    public async Task<int> CountActiveUsersAsync()
    {
        return await _userRepository.CountActiveAsync();
    }

    public async Task<int> GetUserWorkloadAsync(int userId)
    {
        var orders = await _orderRepository.GetAssignedToUserAsync(userId);
        return orders.Count(po => po.CurrentStatus != ProductionStatus.Completed);
    }

    public async Task<bool> IsSetupRequiredAsync()
    {
        var count = await _userRepository.CountAsync();
        return count == 0;
    }

    public async Task<bool> HasActiveOrdersAsync(int userId)
    {
        var orders = await _orderRepository.GetAssignedToUserAsync(userId);
        return orders.Any(o => o.CurrentStatus != ProductionStatus.Completed && o.CurrentStatus != ProductionStatus.Finished);
    }

    public async Task<List<UserDto>> GetUsersForOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return new List<UserDto>();

        var result = new List<User>();

        // 1. Add the main assigned user if exists
        if (order.UserId.HasValue)
        {
            var mainUser = await _userRepository.GetByIdAsync(order.UserId.Value);
            if (mainUser != null && mainUser.IsActive) result.Add(mainUser);
        }

        // 2. Add team members if order is assigned to a team
        if (order.SewingTeamId.HasValue)
        {
            var team = await _teamRepository.GetTeamWithMembersAsync(order.SewingTeamId.Value);
            if (team != null)
            {
                foreach (var member in team.Members)
                {
                    if (member.IsActive && !result.Any(u => u.Id == member.Id))
                    {
                        result.Add(member);
                    }
                }
            }
        }

        return _mapper.ToDtoList(result);
    }
}
