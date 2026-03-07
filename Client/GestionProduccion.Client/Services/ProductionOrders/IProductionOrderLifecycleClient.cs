/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs; // From Shared
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace GestionProduccion.Client.Services.ProductionOrders;

public interface IProductionOrderLifecycleClient
{
    Task<ApiResponse<ProductionOrderDto>> AssignTaskAsync(int orderId, int userId, CancellationToken ct = default);
    Task<ApiResponse<ProductionOrderDto>> AdvanceStageAsync(int orderId, CancellationToken ct = default);
    Task<ApiResponse<bool>> ChangeStageAsync(int orderId, ProductionStage newStage, string note, CancellationToken ct = default);
    Task<ApiResponse<ProductionOrderDto>> UpdateStatusAsync(int orderId, ProductionStatus newStatus, string note, CancellationToken ct = default);
    Task<ApiResponse<BulkUpdateResult>> BulkUpdateStatusAsync(List<int> orderIds, ProductionStatus newStatus, string note, CancellationToken ct = default);
    Task<ApiResponse<bool>> RegisterPartialOutputAsync(int orderId, Dictionary<int, int> sizeOutputs, CancellationToken ct = default);
}


