using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Domain.Interfaces.Repositories;

public interface IProductionOrderRepository
{
    Task<ProductionOrder?> GetByIdAsync(int id);
    Task<ProductionOrder?> GetByUniqueCodeAsync(string uniqueCode);
    Task<List<ProductionOrder>> GetAllAsync();
    Task<IQueryable<ProductionOrder>> GetQueryableAsync(); // For complex filtering in Service
    Task AddAsync(ProductionOrder order);
    Task UpdateAsync(ProductionOrder order);
    Task<int> CountAsync(System.Linq.Expressions.Expression<Func<ProductionOrder, bool>> predicate);
    Task<int> GetCountByStatusAsync(ProductionStatus status);
    Task<List<ProductionOrder>> GetAssignedToUserAsync(int userId);
    Task AddHistoryAsync(ProductionHistory history);
    Task<List<ProductionHistory>> GetHistoryByOrderIdAsync(int orderId);
    Task SaveChangesAsync();
}
