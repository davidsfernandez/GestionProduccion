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

public class ProductionOrderMutationClient : IProductionOrderMutationClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _options;

    public ProductionOrderMutationClient(HttpClient httpClient, JsonSerializerOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<ApiResponse<ProductionOrderDto>> CreateProductionOrderAsync(CreateProductionOrderRequest request, int? assignedUserId = null, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/ProductionOrders", request, _options, ct);
        return await response.Content.ReadFromJsonAsync<ApiResponse<ProductionOrderDto>>(_options, ct)
            ?? ApiResponse<ProductionOrderDto>.FailureResult("Erro ao criar ordem de produção.");
    }

    public async Task<ApiResponse<ProductionOrderDto>> UpdateProductionOrderAsync(int id, UpdateProductionOrderRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/ProductionOrders/{id}", request, _options, ct);
        return await response.Content.ReadFromJsonAsync<ApiResponse<ProductionOrderDto>>(_options, ct)
            ?? ApiResponse<ProductionOrderDto>.FailureResult("Erro ao atualizar ordem de produção.");
    }

    public async Task<ApiResponse<bool>> ArchiveProductionOrderAsync(int id, CancellationToken ct = default)
    {
        var response = await _httpClient.PatchAsync($"api/ProductionOrders/{id}/archive", null, ct);
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(_options, ct)
            ?? ApiResponse<bool>.FailureResult("Erro ao arquivar ordem de produção.");
    }

    public async Task<ApiResponse<bool>> DeleteProductionOrderAsync(int id, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"api/ProductionOrders/{id}", ct);
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(_options, ct)
            ?? ApiResponse<bool>.FailureResult("Erro ao excluir ordem de produção.");
    }
}
