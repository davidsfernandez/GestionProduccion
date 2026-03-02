using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GestionProduccion.Data.Repositories;

public class UserRefreshTokenRepository : IUserRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public UserRefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(UserRefreshToken token)
    {
        await _context.UserRefreshTokens.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    public async Task<UserRefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.UserRefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task RevokeAllUserTokensAsync(int userId)
    {
        await _context.UserRefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsRevoked, true));
    }

    public async Task UpdateAsync(UserRefreshToken token)
    {
        _context.UserRefreshTokens.Update(token);
        await _context.SaveChangesAsync();
    }
}
