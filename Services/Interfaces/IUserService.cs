using GestionProduccion.Domain.Entities;

namespace GestionProduccion.Services.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Gets an active user by their ID.
    /// </summary>
    Task<User?> GetUserByIdAsync(int userId);

    /// <summary>
    /// Gets an active user by their email address.
    /// </summary>
    Task<User?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Gets all active users in the system.
    /// </summary>
    Task<List<User>> GetActiveUsersAsync();

    /// <summary>
    /// Gets all users with a specific role.
    /// </summary>
    Task<List<User>> GetUsersByRoleAsync(string role);

    /// <summary>
    /// Verifies if a user is assigned to a specific production order.
    /// </summary>
    Task<bool> IsUserAssignedToOrderAsync(int userId, int orderId);

    /// <summary>
    /// Gets all production orders assigned to a user.
    /// </summary>
    Task<List<ProductionOrder>> GetUserAssignedOrdersAsync(int userId);

    /// <summary>
    /// Creates a new user (admin only).
    /// </summary>
    Task<User> CreateUserAsync(User user);

    /// <summary>
    /// Updates the avatar URL for a specific user.
    /// </summary>
    Task<bool> UpdateUserAvatarAsync(int userId, string avatarUrl);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    Task<User> UpdateUserAsync(User user);

    /// <summary>
    /// Deactivates a user (soft delete).
    /// </summary>
    Task<bool> DeactivateUserAsync(int userId);

    /// <summary>
    /// Updates the password for a specific user.
    /// </summary>
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

    /// <summary>
    /// Counts total active users in the system.
    /// </summary>
    Task<int> CountActiveUsersAsync();

    /// <summary>
    /// Gets the workload (count of assigned orders) for a specific user.
    /// </summary>
    Task<int> GetUserWorkloadAsync(int userId);

    /// <summary>
    /// Checks if the system needs initial setup (no users exist).
    /// </summary>
    Task<bool> IsSetupRequiredAsync();
}
