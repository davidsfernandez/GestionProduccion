using GestionProduccion.Data;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GestionProduccion.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Teams)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<User>> GetAllActiveAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.IsActive)
            .ToListAsync();
    }

    public async Task<List<User>> GetByRoleAsync(string role)
    {
        if (Enum.TryParse<UserRole>(role, true, out var userRole))
        {
            return await _context.Users
                .Where(u => u.Role == userRole && u.IsActive)
                .ToListAsync();
        }
        return new List<User>();
    }

    public async Task<bool> ExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email);
    }

    public async Task<int> CountActiveAsync()
    {
        return await _context.Users.CountAsync(u => u.IsActive);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
