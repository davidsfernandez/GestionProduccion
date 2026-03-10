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

namespace GestionProduccion.Client.Services;

public interface IProductClient
{
    Task<ApiResponse<List<ProductDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default);
    Task<ApiResponse<ProductDto>> UpdateAsync(int id, UpdateProductDto dto, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default);
}

public class ProductClient : IProductClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _options;

    public ProductClient(HttpClient http, JsonSerializerOptions options)
    {
        _http = http;
        _options = options;
    }

    public async Task<ApiResponse<List<ProductDto>>> GetAllAsync(CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<ApiResponse<List<ProductDto>>>("api/Products", _options, ct)
            ?? ApiResponse<List<ProductDto>>.FailureResult("Erro ao carregar catálogo.");
    }

    public async Task<ApiResponse<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/Products", dto, _options, ct);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>(_options, ct)
            ?? ApiResponse<ProductDto>.FailureResult("Erro ao criar produto.");
    }

    public async Task<ApiResponse<ProductDto>> UpdateAsync(int id, UpdateProductDto dto, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync($"api/Products/{id}", dto, _options, ct);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>(_options, ct)
            ?? ApiResponse<ProductDto>.FailureResult("Erro ao atualizar produto.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync($"api/Products/{id}", ct);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<bool>>(_options, ct)
            ?? ApiResponse<bool>.FailureResult("Erro ao excluir produto.");
    }
}
