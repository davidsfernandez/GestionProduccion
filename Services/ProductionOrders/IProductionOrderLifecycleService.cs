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
using GestionProduccion.Models.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GestionProduccion.Services.ProductionOrders;

public interface IProductionOrderLifecycleService
{
    Task<ProductionOrderDto?> AssignTaskAsync(int orderId, int userId, CancellationToken ct = default);
    Task<ProductionOrderDto?> UpdateStatusAsync(int orderId, ProductionStatus newStatus, string note, int modifiedByUserId, CancellationToken ct = default);
    Task<BulkUpdateResult> BulkUpdateStatusAsync(List<int> orderIds, ProductionStatus newStatus, string note, int modifiedByUserId, CancellationToken ct = default);
    Task<ProductionOrderDto?> AdvanceStageAsync(int orderId, int modifiedByUserId, CancellationToken ct = default);
    Task<bool> ChangeStageAsync(int orderId, ProductionStage newStage, string note, int modifiedByUserId, CancellationToken ct = default);
    Task<bool> RegisterPartialOutputAsync(int orderId, Dictionary<int, int> sizeOutputs, int modifiedByUserId, CancellationToken ct = default);
}
