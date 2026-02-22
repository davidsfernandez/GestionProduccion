using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GestionProduccion.Data.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly AppDbContext _context;

    public PasswordResetTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PasswordResetToken token)
    {
        await _context.PasswordResetTokens.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    public async Task<PasswordResetToken?> GetByHashAsync(string hash)
    {
        return await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash);
    }

    public async Task UpdateAsync(PasswordResetToken token)
    {
        _context.PasswordResetTokens.Update(token);
        await _context.SaveChangesAsync();
    }
}
