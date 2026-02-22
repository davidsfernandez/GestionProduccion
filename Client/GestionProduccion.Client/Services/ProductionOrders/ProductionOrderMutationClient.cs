using GestionProduccion.Client.Models.DTOs;
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

    public async Task<ProductionOrderDto?> CreateProductionOrderAsync(CreateProductionOrderRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/ProductionOrders", request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductionOrderDto>(cancellationToken: ct);
    }

    public async Task<bool> DeleteProductionOrderAsync(int id, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"api/ProductionOrders/{id}", ct);
        return response.IsSuccessStatusCode;
    }
}
