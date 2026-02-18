using GestionProduccion.Data;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GestionProduccion.Data.Repositories;

public class ProductionOrderRepository : IProductionOrderRepository
{
    private readonly AppDbContext _context;

    public ProductionOrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProductionOrder?> GetByIdAsync(int id)
    {
        return await _context.ProductionOrders
            .Include(po => po.AssignedUser)
            .Include(po => po.History)
            .FirstOrDefaultAsync(po => po.Id == id);
    }

    public async Task<ProductionOrder?> GetByUniqueCodeAsync(string uniqueCode)
    {
        return await _context.ProductionOrders
            .FirstOrDefaultAsync(x => x.UniqueCode == uniqueCode);
    }

    public async Task<List<ProductionOrder>> GetAllAsync()
    {
        return await _context.ProductionOrders
            .Include(po => po.AssignedUser)
            .OrderByDescending(po => po.CreationDate)
            .ToListAsync();
    }

    public Task<IQueryable<ProductionOrder>> GetQueryableAsync()
    {
        // Return Queryable for complex filtering in service layer if needed, 
        // though ideally all filtering moves here. 
        // For now, to ease refactoring, we expose IQueryable.
        return Task.FromResult(_context.ProductionOrders.Include(po => po.AssignedUser).AsQueryable());
    }

    public async Task AddAsync(ProductionOrder order)
    {
        await _context.ProductionOrders.AddAsync(order);
    }

    public Task UpdateAsync(ProductionOrder order)
    {
        _context.ProductionOrders.Update(order);
        return Task.CompletedTask;
    }

    public async Task<int> CountAsync(Expression<Func<ProductionOrder, bool>> predicate)
    {
        return await _context.ProductionOrders.CountAsync(predicate);
    }

    public async Task<int> GetCountByStatusAsync(ProductionStatus status)
    {
        return await _context.ProductionOrders.CountAsync(o => o.CurrentStatus == status);
    }

    public async Task<List<ProductionOrder>> GetAssignedToUserAsync(int userId)
    {
        return await _context.ProductionOrders
            .Where(po => po.UserId == userId)
            .OrderByDescending(po => po.CreationDate)
            .ToListAsync();
    }

    public async Task AddHistoryAsync(ProductionHistory history)
    {
        await _context.ProductionHistories.AddAsync(history);
    }

    public async Task<List<ProductionHistory>> GetHistoryByOrderIdAsync(int orderId)
    {
        return await _context.ProductionHistories
            .Where(h => h.ProductionOrderId == orderId)
            .Include(h => h.ResponsibleUser)
            .OrderByDescending(h => h.ModificationDate)
            .ToListAsync();
    }

    public async Task<List<ProductionHistory>> GetRecentHistoryAsync(int count)
    {
        return await _context.ProductionHistories
            .Include(h => h.ResponsibleUser)
            .Include(h => h.ProductionOrder)
            .OrderByDescending(h => h.ModificationDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
