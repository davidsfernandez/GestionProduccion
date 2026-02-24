using GestionProduccion.Models.DTOs;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GestionProduccion.Client.Services;

public interface IProductClient
{
    Task<List<ProductDto>?> GetAllProductsAsync(CancellationToken ct = default);
}

public class ProductClient : IProductClient
{
    private readonly HttpClient _httpClient;
    public ProductClient(HttpClient httpClient) => _httpClient = httpClient;
    public async Task<List<ProductDto>?> GetAllProductsAsync(CancellationToken ct = default) =>
        await _httpClient.GetFromJsonAsync<List<ProductDto>>("api/Products", ct);
}

public interface ISewingTeamClient
{
    Task<ApiResponse<List<SewingTeamDto>>?> GetAllTeamsAsync(CancellationToken ct = default);
}

public class SewingTeamClient : ISewingTeamClient
{
    private readonly HttpClient _httpClient;
    public SewingTeamClient(HttpClient httpClient) => _httpClient = httpClient;
    public async Task<ApiResponse<List<SewingTeamDto>>?> GetAllTeamsAsync(CancellationToken ct = default) =>
        await _httpClient.GetFromJsonAsync<ApiResponse<List<SewingTeamDto>>>("api/SewingTeams", ct);
}