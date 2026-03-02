using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GestionProduccion.Data.Repositories;

public class BonusRuleRepository : IBonusRuleRepository
{
    private readonly AppDbContext _context;

    public BonusRuleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BonusRule?> GetByIdAsync(int id)
    {
        return await _context.BonusRules.FindAsync(id);
    }

    public async Task<List<BonusRule>> GetAllAsync()
    {
        return await _context.BonusRules.ToListAsync();
    }

    public async Task DeleteAsync(BonusRule rule)
    {
        _context.BonusRules.Remove(rule);
        await Task.CompletedTask;
    }

    public async Task<BonusRule?> GetActiveRuleAsync()
    {
        return await _context.BonusRules
            .FirstOrDefaultAsync(r => r.IsActive);
    }

    public async Task AddAsync(BonusRule rule)
    {
        await _context.BonusRules.AddAsync(rule);
    }

    public Task UpdateAsync(BonusRule rule)
    {
        _context.BonusRules.Update(rule);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
