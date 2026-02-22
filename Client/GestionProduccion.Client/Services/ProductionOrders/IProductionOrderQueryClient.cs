using GestionProduccion.Client.Models.DTOs;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace GestionProduccion.Client.Services.ProductionOrders;

public interface IProductionOrderQueryClient
{
    Task<ProductionOrderDto?> GetProductionOrderByIdAsync(int id, CancellationToken ct = default);
    Task<List<ProductionOrderDto>?> ListProductionOrdersAsync(FilterProductionOrderDto? filter, CancellationToken ct = default);
    Task<DashboardDto?> GetDashboardAsync(CancellationToken ct = default);
    Task<List<ProductionHistoryDto>?> GetHistoryByProductionOrderIdAsync(int orderId, CancellationToken ct = default);
}
