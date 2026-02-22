using GestionProduccion.Client.Models.DTOs;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Enums;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GestionProduccion.Client.Services.ProductionOrders;

public class ProductionOrderLifecycleClient : IProductionOrderLifecycleClient
{
    private readonly HttpClient _httpClient;

    public ProductionOrderLifecycleClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> AssignTaskAsync(int orderId, int userId, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/ProductionOrders/{orderId}/assign", new { UserId = userId }, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> AdvanceStageAsync(int orderId, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsync($"api/ProductionOrders/{orderId}/advance-stage", null, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ChangeStageAsync(int orderId, ProductionStage newStage, string note, CancellationToken ct = default)
    {
        var request = new ChangeStageRequest { NewStage = newStage, Note = note };
        var response = await _httpClient.PostAsJsonAsync($"api/ProductionOrders/{orderId}/change-stage", request, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateStatusAsync(int orderId, ProductionStatus newStatus, string note, CancellationToken ct = default)
    {
        var request = new UpdateStatusRequest { NewStatus = newStatus, Note = note };
        var response = await _httpClient.PatchAsJsonAsync($"api/ProductionOrders/{orderId}/status", request, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<BulkUpdateResult?> BulkUpdateStatusAsync(List<int> orderIds, ProductionStatus newStatus, string note, CancellationToken ct = default)
    {
        var request = new BulkUpdateStatusRequest { OrderIds = orderIds, NewStatus = newStatus, Note = note };
        var response = await _httpClient.PostAsJsonAsync("api/ProductionOrders/bulk-status", request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BulkUpdateResult>(cancellationToken: ct);
    }
}
