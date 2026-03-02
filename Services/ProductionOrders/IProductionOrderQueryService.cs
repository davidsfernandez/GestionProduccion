using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GestionProduccion.Services.ProductionOrders;

public interface IProductionOrderQueryService
{
    Task<ProductionOrderDto?> GetProductionOrderByIdAsync(int id, CancellationToken ct = default);
    Task<List<ProductionOrderDto>> ListProductionOrdersAsync(FilterProductionOrderDto? filter, CancellationToken ct = default);
    Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default);
    Task<List<ProductionHistoryDto>> GetHistoryByProductionOrderIdAsync(int orderId, CancellationToken ct = default);
    Task<List<ProductionOrderDto>> GetTeamProductionOrdersAsync(int userId, CancellationToken ct = default);
}
