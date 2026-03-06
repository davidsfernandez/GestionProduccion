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
using System.Net.Http.Json;
using System.Web;

namespace GestionProduccion.Client.Services.ProductionOrders;

public class ProductionOrderQueryClient : IProductionOrderQueryClient
{
    private readonly HttpClient _httpClient;

    public ProductionOrderQueryClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<ProductionOrderDto>?> GetProductionOrderByIdAsync(int id, CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<ApiResponse<ProductionOrderDto>>($"api/ProductionOrders/{id}", ct);
    }

    public async Task<ApiResponse<PaginatedResponseDto<ProductionOrderDto>>?> ListProductionOrdersAsync(FilterProductionOrderDto? filter, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["page"] = page.ToString();
        query["pageSize"] = pageSize.ToString();

        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.SearchTerm)) query["SearchTerm"] = filter.SearchTerm;
            if (!string.IsNullOrEmpty(filter.CurrentStage)) query["CurrentStage"] = filter.CurrentStage;
            if (!string.IsNullOrEmpty(filter.CurrentStatus)) query["CurrentStatus"] = filter.CurrentStatus;
            if (filter.UserId.HasValue) query["UserId"] = filter.UserId.ToString();
            if (filter.StartDate.HasValue) query["StartDate"] = filter.StartDate.Value.ToString("yyyy-MM-dd");
            if (filter.EndDate.HasValue) query["EndDate"] = filter.EndDate.Value.ToString("yyyy-MM-dd");
        }

        return await _httpClient.GetFromJsonAsync<ApiResponse<PaginatedResponseDto<ProductionOrderDto>>>($"api/ProductionOrders?{query}", ct);
    }

    public async Task<ApiResponse<DashboardDto>?> GetDashboardAsync(CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<ApiResponse<DashboardDto>>("api/ProductionOrders/dashboard", ct);
    }

    public async Task<ApiResponse<List<ProductionHistoryDto>>?> GetHistoryByProductionOrderIdAsync(int orderId, CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<ApiResponse<List<ProductionHistoryDto>>>($"api/ProductionOrders/{orderId}/history", ct);
    }
}
