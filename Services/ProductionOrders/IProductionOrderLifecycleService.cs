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
    Task<bool> AssignTaskAsync(int orderId, int userId, CancellationToken ct = default);
    Task<bool> AdvanceStageAsync(int orderId, int modifiedByUserId, CancellationToken ct = default);
    Task<bool> ChangeStageAsync(int orderId, ProductionStage newStage, string note, int modifiedByUserId, CancellationToken ct = default);
    Task<bool> UpdateStatusAsync(int orderId, ProductionStatus newStatus, string note, int modifiedByUserId, CancellationToken ct = default);
    Task<BulkUpdateResult> BulkUpdateStatusAsync(List<int> orderIds, ProductionStatus newStatus, string note, int modifiedByUserId, CancellationToken ct = default);
    
    /// <summary>
    /// Registers partial completion of items in a production order for its current stage.
    /// </summary>
    /// <param name="orderId">The production order ID.</param>
    /// <param name="sizeOutputs">A dictionary where key is SizeId and value is Quantity to complete.</param>
    /// <param name="modifiedByUserId">The user performing the action.</param>
    /// <returns>True if at least one output was registered successfully.</returns>
    Task<bool> RegisterPartialOutputAsync(int orderId, Dictionary<int, int> sizeOutputs, int modifiedByUserId, CancellationToken ct = default);
}

