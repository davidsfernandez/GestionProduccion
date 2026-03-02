using GestionProduccion.Models.DTOs;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace GestionProduccion.Client.Services;

public interface IProductClient
{
    Task<List<ProductDto>?> GetAllProductsAsync(CancellationToken ct = default);
}

public class ProductClient : IProductClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _options;

    public ProductClient(HttpClient httpClient, JsonSerializerOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<List<ProductDto>?> GetAllProductsAsync(CancellationToken ct = default) =>
        await _httpClient.GetFromJsonAsync<List<ProductDto>>("api/Products", _options, ct);
}

public interface ISewingTeamClient
{
    Task<ApiResponse<List<SewingTeamDto>>?> GetAllTeamsAsync(CancellationToken ct = default);
}

public class SewingTeamClient : ISewingTeamClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _options;

    public SewingTeamClient(HttpClient httpClient, JsonSerializerOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<ApiResponse<List<SewingTeamDto>>?> GetAllTeamsAsync(CancellationToken ct = default) =>
        await _httpClient.GetFromJsonAsync<ApiResponse<List<SewingTeamDto>>>("api/SewingTeams", _options, ct);
}