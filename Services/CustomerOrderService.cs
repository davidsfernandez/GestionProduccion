using GestionProduccion.Data;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Application.Mapping;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GestionProduccion.Services;

public class CustomerOrderService : ICustomerOrderService
{
    private readonly AppDbContext _context;

    public CustomerOrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductionOrderDto>> GetCustomerOrdersAsync(int customerUserId, CancellationToken ct = default)
    {
        var orders = await _context.ProductionOrders
            .Include(o => o.Product)
            .Include(o => o.Sizes)
            .Where(o => o.CustomerUserId == customerUserId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

        return orders.Select(o => o.ToDto()).ToList();
    }

    public async Task<ProductionOrderDto> GetCustomerOrderDetailsAsync(int orderId, int customerUserId, CancellationToken ct = default)
    {
        var order = await _context.ProductionOrders
            .Include(o => o.Product)
            .Include(o => o.Sizes)
            .Include(o => o.History)
                .ThenInclude(h => h.ResponsibleUser)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerUserId == customerUserId, ct)
            ?? throw new KeyNotFoundException("Order not found or access denied.");

        return order.ToDto();
    }
}