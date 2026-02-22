using GestionProduccion.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace GestionProduccion.Client.Services.ProductionOrders;

public interface IProductionOrderMutationClient
{
    Task<ProductionOrderDto?> CreateProductionOrderAsync(CreateProductionOrderRequest request, int? assignedUserId = null, CancellationToken ct = default);
    Task<bool> DeleteProductionOrderAsync(int id, CancellationToken ct = default);
}
