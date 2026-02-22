using GestionProduccion.Models.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GestionProduccion.Services.ProductionOrders;

public interface IProductionOrderMutationService
{
    Task<ProductionOrderDto> CreateProductionOrderAsync(CreateProductionOrderRequest request, int createdByUserId, CancellationToken ct = default);
    Task<bool> DeleteProductionOrderAsync(int id, CancellationToken ct = default);
    // Any other direct update methods not related to state changes would go here, e.g., UpdateProductionOrderMetadataAsync
}
