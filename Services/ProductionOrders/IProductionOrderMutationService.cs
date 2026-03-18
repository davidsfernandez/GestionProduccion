/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using GestionProduccion.Models.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GestionProduccion.Services.ProductionOrders;

public interface IProductionOrderMutationService
{
    Task<ProductionOrderDto> CreateProductionOrderAsync(CreateProductionOrderRequest request, int createdByUserId, CancellationToken ct = default);
    Task<ProductionOrderDto> UpdateProductionOrderAsync(UpdateProductionOrderRequest request, int modifiedByUserId, CancellationToken ct = default);
    Task<bool> ArchiveProductionOrderAsync(int id, int userId, CancellationToken ct = default);
    Task<bool> DeleteProductionOrderAsync(int id, CancellationToken ct = default);
}
