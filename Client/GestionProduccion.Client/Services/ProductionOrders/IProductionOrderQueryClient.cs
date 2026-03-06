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

public interface IProductionOrderQueryClient
{
    Task<ApiResponse<ProductionOrderDto>?> GetProductionOrderByIdAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<PaginatedResponseDto<ProductionOrderDto>>?> ListProductionOrdersAsync(FilterProductionOrderDto? filter, int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse<DashboardDto>?> GetDashboardAsync(CancellationToken ct = default);
    Task<ApiResponse<List<ProductionHistoryDto>>?> GetHistoryByProductionOrderIdAsync(int orderId, CancellationToken ct = default);
}


