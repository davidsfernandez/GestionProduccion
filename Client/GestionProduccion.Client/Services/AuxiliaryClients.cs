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
using System.Text.Json;

namespace GestionProduccion.Client.Services;

public interface IProductClient
{
    Task<ApiResponse<List<ProductDto>>?> GetAllProductsAsync(CancellationToken ct = default);
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

    public async Task<ApiResponse<List<ProductDto>>?> GetAllProductsAsync(CancellationToken ct = default) =>
        await _httpClient.GetFromJsonAsync<ApiResponse<List<ProductDto>>>("api/Products", _options, ct);
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
