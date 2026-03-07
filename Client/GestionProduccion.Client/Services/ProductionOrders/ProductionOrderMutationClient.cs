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
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GestionProduccion.Client.Services.ProductionOrders;

public class ProductionOrderMutationClient : IProductionOrderMutationClient
{
    private readonly HttpClient _httpClient;

    public ProductionOrderMutationClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<ProductionOrderDto>> CreateProductionOrderAsync(CreateProductionOrderRequest request, int? assignedUserId = null, CancellationToken ct = default)
    {
        var url = assignedUserId.HasValue ? $"api/ProductionOrders?userId={assignedUserId.Value}" : "api/ProductionOrders";
        var response = await _httpClient.PostAsJsonAsync(url, request, ct);
        return await response.Content.ReadFromJsonAsync<ApiResponse<ProductionOrderDto>>(cancellationToken: ct)
            ?? ApiResponse<ProductionOrderDto>.FailureResult("Erro ao criar ordem de produção.");
    }

    public async Task<ApiResponse<bool>> DeleteProductionOrderAsync(int id, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"api/ProductionOrders/{id}", ct);
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: ct)
            ?? ApiResponse<bool>.FailureResult("Erro ao excluir ordem de produção.");
    }
}


