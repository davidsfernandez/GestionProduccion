using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Domain.Interfaces.Repositories;

public interface IProductionOrderOutputRepository
{
    Task AddAsync(ProductionOrderOutput output);
    Task<IEnumerable<ProductionOrderOutput>> GetByOrderIdAsync(int orderId);
    Task<IEnumerable<ProductionOrderOutput>> GetByTeamAndDateRangeAsync(int teamId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<ProductionOrderOutput>> GetByUserAndDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
    Task<int> GetTotalQuantityByOrderAndStageAsync(int orderId, ProductionStage stage);
    Task SaveChangesAsync();
}
