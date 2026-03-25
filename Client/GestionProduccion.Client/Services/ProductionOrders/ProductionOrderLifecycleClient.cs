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
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Client.Services.ProductionOrders;

public class ProductionOrderLifecycleClient : IProductionOrderLifecycleClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _options;

    public ProductionOrderLifecycleClient(HttpClient httpClient, JsonSerializerOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<ApiResponse<ProductionOrderDto>> AssignTaskAsync(int orderId, int userId, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/ProductionOrders/{orderId}/assign", new { UserId = userId }, _options, ct);
        return await response.Content.ReadFromJsonAsync<ApiResponse<ProductionOrderDto>>(_options, ct)
            ?? ApiResponse<ProductionOrderDto>.FailureResult("Erro ao atribuir operador.");
    }

    public async Task<ApiResponse<ProductionOrderDto>> UpdateStatusAsync(int orderId, ProductionStatus newStatus, string note, CancellationToken ct = default)
    {
        var response = await _httpClient.PatchAsJsonAsync($"api/ProductionOrders/{orderId}/status", new { NewStatus = newStatus, Note = note }, _options, ct);
        return await response.Content.ReadFromJsonAsync<ApiResponse<ProductionOrderDto>>(_options, ct)
            ?? ApiResponse<ProductionOrderDto>.FailureResult("Erro ao atualizar status.");
    }

    public async Task<ApiResponse<ProductionOrderDto>> AdvanceStageAsync(int orderId, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsync($"api/ProductionOrders/{orderId}/advance-stage", null, ct);
        return await response.Content.ReadFromJsonAsync<ApiResponse<ProductionOrderDto>>(_options, ct)
            ?? ApiResponse<ProductionOrderDto>.FailureResult("Erro ao avançar etapa.");
    }

    public async Task<ApiResponse<BulkUpdateResult>> BulkAdvanceStageAsync(List<int> orderIds, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/ProductionOrders/bulk-advance-stage", new { OrderIds = orderIds }, _options, ct);
        return await response.Content.ReadFromJsonAsync<ApiResponse<BulkUpdateResult>>(_options, ct)
            ?? ApiResponse<BulkUpdateResult>.FailureResult("Erro ao avançar etapas em massa.");
    }

    public async Task<ApiResponse<bool>> ChangeStageAsync(int orderId, ProductionStage newStage, string note, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/ProductionOrders/{orderId}/change-stage", new { NewStage = newStage, Note = note }, _options, ct);
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(_options, ct)
            ?? ApiResponse<bool>.FailureResult("Erro ao alterar etapa.");
    }

    public async Task<ApiResponse<BulkUpdateResult>> BulkChangeStageAsync(List<int> orderIds, ProductionStage newStage, string note, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/ProductionOrders/bulk-change-stage", new { OrderIds = orderIds, NewStage = newStage, Note = note }, _options, ct);
        return await response.Content.ReadFromJsonAsync<ApiResponse<BulkUpdateResult>>(_options, ct)
            ?? ApiResponse<BulkUpdateResult>.FailureResult("Erro ao alterar etapas em massa.");
    }

    public async Task<ApiResponse<BulkUpdateResult>> BulkUpdateStatusAsync(List<int> orderIds, ProductionStatus newStatus, string note, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/ProductionOrders/bulk-status", new { OrderIds = orderIds, NewStatus = newStatus, Note = note }, _options, ct);
        return await response.Content.ReadFromJsonAsync<ApiResponse<BulkUpdateResult>>(_options, ct)
            ?? ApiResponse<BulkUpdateResult>.FailureResult("Erro ao processar atualização em massa.");
    }

    public async Task<ApiResponse<bool>> RegisterPartialOutputAsync(int orderId, Dictionary<int, int> sizeOutputs, CancellationToken ct = default)
    {
        var request = new { SizeOutputs = sizeOutputs };
        var response = await _httpClient.PostAsJsonAsync($"api/ProductionOrders/{orderId}/partial-output", request, _options, ct);
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(_options, ct)
            ?? ApiResponse<bool>.FailureResult("Erro ao registrar produção parcial.");
    }
}
