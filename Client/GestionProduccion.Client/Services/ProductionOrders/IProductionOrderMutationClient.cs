using GestionProduccion.Client.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace GestionProduccion.Client.Services.ProductionOrders;

public interface IProductionOrderMutationClient
{
    Task<ProductionOrderDto?> CreateProductionOrderAsync(CreateProductionOrderRequest request, CancellationToken ct = default);
    Task<bool> DeleteProductionOrderAsync(int id, CancellationToken ct = default);
    // Any other direct update methods not related to state changes would go here
}
