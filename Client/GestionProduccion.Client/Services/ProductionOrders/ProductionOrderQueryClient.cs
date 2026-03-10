/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited. 
 * 
 * Proprietary and Confidential.
 */

using System.Net.Http.Json;
using System.Text.Json;
using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Client.Services.ProductionOrders;

public class ProductionOrderQueryClient : IProductionOrderQueryClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _options;

    public ProductionOrderQueryClient(HttpClient httpClient, JsonSerializerOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<ApiResponse<ProductionOrderDto>?> GetProductionOrderByIdAsync(int id, CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<ApiResponse<ProductionOrderDto>>($"api/ProductionOrders/{id}", _options, ct);
    }

    public async Task<ApiResponse<PaginatedResponseDto<ProductionOrderDto>>?> ListProductionOrdersAsync(FilterProductionOrderDto? filter, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var url = $"api/ProductionOrders?page={page}&pageSize={pageSize}";
        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.SearchTerm)) url += $"&SearchTerm={filter.SearchTerm}";
            if (!string.IsNullOrEmpty(filter.CurrentStage)) url += $"&CurrentStage={filter.CurrentStage}";
            if (!string.IsNullOrEmpty(filter.CurrentStatus)) url += $"&CurrentStatus={filter.CurrentStatus}";
            if (!string.IsNullOrEmpty(filter.ClientName)) url += $"&ClientName={filter.ClientName}";
            if (!string.IsNullOrEmpty(filter.Size)) url += $"&Size={filter.Size}";
        }
        return await _httpClient.GetFromJsonAsync<ApiResponse<PaginatedResponseDto<ProductionOrderDto>>>(url, _options, ct);
    }

    public async Task<ApiResponse<List<ProductionHistoryDto>>?> GetHistoryByProductionOrderIdAsync(int orderId, CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<ApiResponse<List<ProductionHistoryDto>>>($"api/ProductionOrders/{orderId}/history", _options, ct);
    }

    public async Task<ApiResponse<DashboardDto>?> GetDashboardAsync(CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<ApiResponse<DashboardDto>>("api/ProductionOrders/dashboard", _options, ct);
    }

    public async Task<ApiResponse<List<ProductionOrderDto>>?> GetTeamProductionOrdersAsync(int userId, CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<ApiResponse<List<ProductionOrderDto>>>($"api/Tasks/my", _options, ct);
    }
}
