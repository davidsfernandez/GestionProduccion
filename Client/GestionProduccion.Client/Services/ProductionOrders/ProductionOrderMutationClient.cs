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

    public async Task<ProductionOrderDto?> CreateProductionOrderAsync(CreateProductionOrderRequest request, int? assignedUserId = null, CancellationToken ct = default)
    {
        var url = assignedUserId.HasValue ? $"api/ProductionOrders?userId={assignedUserId.Value}" : "api/ProductionOrders";
        var response = await _httpClient.PostAsJsonAsync(url, request, ct);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ProductionOrderDto>(cancellationToken: ct);
        }
        return null;
    }

    public async Task<bool> DeleteProductionOrderAsync(int id, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"api/ProductionOrders/{id}", ct);
        return response.IsSuccessStatusCode;
    }
}
