using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs; // From Shared
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace GestionProduccion.Client.Services.ProductionOrders;

public class ProductionOrderQueryClient : IProductionOrderQueryClient
{
    private readonly HttpClient _httpClient;

    public ProductionOrderQueryClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductionOrderDto?> GetProductionOrderByIdAsync(int id, CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<ProductionOrderDto>($"api/ProductionOrders/{id}", ct);
    }

    public async Task<List<ProductionOrderDto>?> ListProductionOrdersAsync(FilterProductionOrderDto? filter, CancellationToken ct = default)
    {
        var queryParams = new List<string>();
        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.CurrentStage)) queryParams.Add($"CurrentStage={filter.CurrentStage}");
            if (!string.IsNullOrWhiteSpace(filter.CurrentStatus)) queryParams.Add($"CurrentStatus={filter.CurrentStatus}");
            if (filter.UserId.HasValue) queryParams.Add($"UserId={filter.UserId.Value}");
            if (filter.StartDate.HasValue) queryParams.Add($"StartDate={filter.StartDate.Value:yyyy-MM-dd}");
            if (filter.EndDate.HasValue) queryParams.Add($"EndDate={filter.EndDate.Value:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(filter.ClientName)) queryParams.Add($"ClientName={filter.ClientName}");
            if (!string.IsNullOrWhiteSpace(filter.Size)) queryParams.Add($"Size={filter.Size}");
        }
        var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
        return await _httpClient.GetFromJsonAsync<List<ProductionOrderDto>>($"api/ProductionOrders{queryString}", ct);
    }

    public async Task<DashboardDto?> GetDashboardAsync(CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<DashboardDto>("api/ProductionOrders/dashboard", ct);
    }

    public async Task<List<ProductionHistoryDto>?> GetHistoryByProductionOrderIdAsync(int orderId, CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<List<ProductionHistoryDto>>($"api/ProductionOrders/{orderId}/history", ct);
    }
}
