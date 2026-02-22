using Microsoft.EntityFrameworkCore;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Interfaces.Repositories;

namespace GestionProduccion.Data.Repositories;

public class BonusRuleRepository : Repository<BonusRule>, IBonusRuleRepository
{
    public BonusRuleRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<BonusRule?> GetActiveRuleAsync()
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.IsActive);
    }
}
