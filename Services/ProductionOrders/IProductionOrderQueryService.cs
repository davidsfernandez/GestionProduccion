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

public interface IProductionOrderQueryService
{
    Task<ProductionOrderDto?> GetProductionOrderByIdAsync(int id, CancellationToken ct = default);
    Task<List<ProductionOrderDto>> ListProductionOrdersAsync(FilterProductionOrderDto? filter, CancellationToken ct = default);
    Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default);
    Task<List<ProductionHistoryDto>> GetHistoryByProductionOrderIdAsync(int orderId, CancellationToken ct = default);
    Task<List<ProductionOrderDto>> GetTeamProductionOrdersAsync(int userId, CancellationToken ct = default);
}

