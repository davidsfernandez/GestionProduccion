using GestionProduccion.Data;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestionProduccion.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets an active user by their ID.
    /// </summary>
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.AssignedOrders)
            .Include(u => u.HistoryChanges)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
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

        return await _context.Users
            .Include(u => u.AssignedOrders)
            .Include(u => u.HistoryChanges)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
    }

    /// <summary>
    /// Gets all active users in the system.
    /// </summary>
    public async Task<List<User>> GetActiveUsersAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.IsActive)
            .Include(u => u.AssignedOrders)
            .OrderBy(u => u.Name)
            .ToListAsync();
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

        return await _context.Users
            .AsNoTracking()
            .Where(u => u.IsActive && u.Role.ToString() == role)
            .Include(u => u.AssignedOrders)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Verifies if a user is assigned to a specific production order.
    /// </summary>
    public async Task<bool> IsUserAssignedToOrderAsync(int userId, int orderId)
    {
        return await _context.ProductionOrders
            .AnyAsync(po => po.Id == orderId && po.UserId == userId && po.AssignedUser!.IsActive);
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

        return await _context.ProductionOrders
            .Where(po => po.UserId == userId)
            .OrderByDescending(po => po.CreationDate)
            .ToListAsync();
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
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with email '{user.Email}' already exists.");
        }

        user.IsActive = true;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> UpdateUserAvatarAsync(int userId, string avatarUrl)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.AvatarUrl = avatarUrl;
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
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

        var existingUser = await _context.Users.FindAsync(user.Id);
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

        _context.Entry(existingUser).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return existingUser;
    }

    /// <summary>
    /// Deactivates a user (soft delete).
    /// </summary>
    public async Task<bool> DeactivateUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        user.IsActive = false;
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Counts total active users in the system.
    /// </summary>
    public async Task<int> CountActiveUsersAsync()
    {
        return await _context.Users.CountAsync(u => u.IsActive);
    }

    /// <summary>
    /// Gets the workload (count of assigned orders) for a specific user.
    /// </summary>
    public async Task<int> GetUserWorkloadAsync(int userId)
    {
        return await _context.ProductionOrders
            .CountAsync(po => po.UserId == userId && po.CurrentStatus != Domain.Enums.ProductionStatus.Completed);
    }
}
