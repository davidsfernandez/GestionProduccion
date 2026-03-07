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
using System.Net.Http.Json;

namespace GestionProduccion.Client.Services.ProductionOrders;

public class ProductionOrderLifecycleClient : IProductionOrderLifecycleClient
{
    private readonly HttpClient _httpClient;

    public ProductionOrderLifecycleClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<ProductionOrderDto>> AssignTaskAsync(int orderId, int userId, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/ProductionOrders/{orderId}/assign", new { UserId = userId }, ct);
        return await response.Content.ReadFromJsonAsync<ApiResponse<ProductionOrderDto>>(cancellationToken: ct) 
            ?? ApiResponse<ProductionOrderDto>.FailureResult("Erro ao atribuir tarefa.");
    }

    public async Task<ApiResponse<ProductionOrderDto>> AdvanceStageAsync(int orderId, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsync($"api/ProductionOrders/{orderId}/advance-stage", null, ct);
        return await response.Content.ReadFromJsonAsync<ApiResponse<ProductionOrderDto>>(cancellationToken: ct)
            ?? ApiResponse<ProductionOrderDto>.FailureResult("Erro ao avançar estágio.");
    }

    public async Task<ApiResponse<bool>> ChangeStageAsync(int orderId, ProductionStage newStage, string note, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/ProductionOrders/{orderId}/change-stage", new { NewStage = newStage, Note = note }, ct);
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: ct)
            ?? ApiResponse<bool>.FailureResult("Erro ao alterar estágio.");
    }

    public async Task<ApiResponse<ProductionOrderDto>> UpdateStatusAsync(int orderId, ProductionStatus newStatus, string note, CancellationToken ct = default)
    {
        var response = await _httpClient.PatchAsJsonAsync($"api/ProductionOrders/{orderId}/status", new { NewStatus = newStatus, Note = note }, ct);
        return await response.Content.ReadFromJsonAsync<ApiResponse<ProductionOrderDto>>(cancellationToken: ct)
            ?? ApiResponse<ProductionOrderDto>.FailureResult("Erro ao atualizar status.");
    }

    public async Task<ApiResponse<BulkUpdateResult>> BulkUpdateStatusAsync(List<int> orderIds, ProductionStatus newStatus, string note, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/ProductionOrders/bulk-status", new { OrderIds = orderIds, NewStatus = newStatus, Note = note }, ct);
        return await response.Content.ReadFromJsonAsync<ApiResponse<BulkUpdateResult>>(cancellationToken: ct)
            ?? ApiResponse<BulkUpdateResult>.FailureResult("Erro ao processar atualização em massa.");
    }

    public async Task<ApiResponse<bool>> RegisterPartialOutputAsync(int orderId, Dictionary<int, int> sizeOutputs, CancellationToken ct = default)
    {
        var request = new PartialOutputRequest { SizeOutputs = sizeOutputs, Note = "Partial output via UI" };
        var response = await _httpClient.PostAsJsonAsync($"api/ProductionOrders/{orderId}/partial-output", request, ct);
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: ct)
            ?? ApiResponse<bool>.FailureResult("Erro ao registrar produção parcial.");
    }
}
