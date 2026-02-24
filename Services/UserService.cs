using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace GestionProduccion.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IProductionOrderRepository _orderRepository;
    private readonly IPasswordResetTokenRepository _passwordResetRepo;
    private readonly IUserRefreshTokenRepository _refreshTokenRepo;

    public UserService(
        IUserRepository userRepository, 
        IProductionOrderRepository orderRepository,
        IPasswordResetTokenRepository passwordResetRepo,
        IUserRefreshTokenRepository refreshTokenRepo)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
        _passwordResetRepo = passwordResetRepo;
        _refreshTokenRepo = refreshTokenRepo;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user != null && user.IsActive ? user : null;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        }

        var user = await _userRepository.GetByEmailAsync(email);
        return user != null && user.IsActive ? user : null;
    }

    public async Task<List<User>> GetActiveUsersAsync()
    {
        return await _userRepository.GetAllActiveAsync();
    }

    public async Task<List<User>> GetUsersByRoleAsync(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role cannot be empty.", nameof(role));
        }

        return await _userRepository.GetByRoleAsync(role);
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

    public async Task<List<ProductionOrder>> GetUserAssignedOrdersAsync(int userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        return await _orderRepository.GetAssignedToUserAsync(userId);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (string.IsNullOrWhiteSpace(user.FullName))
        {
            throw new InvalidOperationException("User name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            throw new InvalidOperationException("User email cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            throw new InvalidOperationException("User password hash cannot be empty.");
        }

        var existingUser = await _userRepository.GetByEmailAsync(user.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with email '{user.Email}' already exists.");
        }

        user.IsActive = true;
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();
        return user;
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

    public async Task<User> UpdateUserAsync(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var existingUser = await _userRepository.GetByIdAsync(user.Id);
        if (existingUser == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        existingUser.FullName = user.FullName;
        existingUser.Email = user.Email;
        existingUser.Role = user.Role;
        existingUser.AvatarUrl = user.AvatarUrl;
        existingUser.IsActive = user.IsActive;

        await _userRepository.UpdateAsync(existingUser);
        await _userRepository.SaveChangesAsync();
        return existingUser;
    }

    public async Task<bool> DeactivateUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        user.IsActive = false;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<string?> RequestPasswordResetAsync(string email)
    {
        var user = await GetUserByEmailAsync(email);
        if (user == null) return null;

        var token = GenerateSecureToken();
        var tokenHash = ComputeHash(token);

        await _passwordResetRepo.AddAsync(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiryDate = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        });

        return token;
    }

    public async Task<bool> CompletePasswordResetAsync(string email, string token, string newPassword)
    {
        var tokenHash = ComputeHash(token);
        var resetToken = await _passwordResetRepo.GetByHashAsync(tokenHash);

        if (resetToken == null || resetToken.IsUsed || resetToken.ExpiryDate <= DateTime.UtcNow)
        {
            return false;
        }

        if (resetToken.User.Email.ToLower() != email.ToLower())
        {
            return false;
        }

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
        return orders.Count(po => po.CurrentStatus != Domain.Enums.ProductionStatus.Completed);
    }

    public async Task<bool> IsSetupRequiredAsync()
    {
        var count = await _userRepository.CountActiveAsync(); 
        return count == 0;
    }
}
