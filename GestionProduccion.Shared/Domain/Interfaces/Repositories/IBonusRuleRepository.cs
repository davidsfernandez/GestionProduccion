using GestionProduccion.Domain.Entities;

namespace GestionProduccion.Domain.Interfaces.Repositories;

public interface IBonusRuleRepository : IRepository<BonusRule>
{
    Task<BonusRule?> GetActiveRuleAsync();
}
