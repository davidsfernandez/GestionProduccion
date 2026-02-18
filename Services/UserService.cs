using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Services.Interfaces;

namespace GestionProduccion.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IProductionOrderRepository _orderRepository;

    public UserService(IUserRepository userRepository, IProductionOrderRepository orderRepository)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Gets an active user by their ID.
    /// </summary>
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user != null && user.IsActive ? user : null;
    }

    /// <summary>
    /// Gets an active user by their email address.
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        }

        var user = await _userRepository.GetByEmailAsync(email);
        return user != null && user.IsActive ? user : null;
    }

    /// <summary>
    /// Gets all active users in the system.
    /// </summary>
    public async Task<List<User>> GetActiveUsersAsync()
    {
        return await _userRepository.GetAllActiveAsync();
    }

    /// <summary>
    /// Gets all users with a specific role.
    /// </summary>
    public async Task<List<User>> GetUsersByRoleAsync(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role cannot be empty.", nameof(role));
        }

        return await _userRepository.GetByRoleAsync(role);
    }

    /// <summary>
    /// Verifies if a user is assigned to a specific production order.
    /// </summary>
    public async Task<bool> IsUserAssignedToOrderAsync(int userId, int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null || order.UserId != userId || order.AssignedUser == null || !order.AssignedUser.IsActive)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Gets all production orders assigned to a user.
    /// </summary>
    public async Task<List<ProductionOrder>> GetUserAssignedOrdersAsync(int userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        return await _orderRepository.GetAssignedToUserAsync(userId);
    }

    /// <summary>
    /// Creates a new user (admin only).
    /// </summary>
    public async Task<User> CreateUserAsync(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        // Validate user data
        if (string.IsNullOrWhiteSpace(user.Name))
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

        // Check if email already exists
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

    /// <summary>
    /// Updates an existing user.
    /// </summary>
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

        existingUser.Name = user.Name;
        existingUser.Email = user.Email;
        existingUser.Role = user.Role;
        existingUser.PublicId = user.PublicId;
        existingUser.AvatarUrl = user.AvatarUrl;
        existingUser.IsActive = user.IsActive;

        await _userRepository.UpdateAsync(existingUser);
        await _userRepository.SaveChangesAsync();
        return existingUser;
    }

    /// <summary>
    /// Deactivates a user (soft delete).
    /// </summary>
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

    /// <summary>
    /// Counts total active users in the system.
    /// </summary>
    public async Task<int> CountActiveUsersAsync()
    {
        return await _userRepository.CountActiveAsync();
    }

    /// <summary>
    /// Gets the workload (count of assigned orders) for a specific user.
    /// </summary>
    public async Task<int> GetUserWorkloadAsync(int userId)
    {
        var orders = await _orderRepository.GetAssignedToUserAsync(userId);
        return orders.Count(po => po.CurrentStatus != Domain.Enums.ProductionStatus.Completed);
    }
}
