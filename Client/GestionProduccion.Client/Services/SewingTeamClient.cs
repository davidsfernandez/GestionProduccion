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

public interface ISewingTeamClient
{
    Task<ApiResponse<List<SewingTeamDto>>> GetAllTeamsAsync(CancellationToken ct = default);
    Task<ApiResponse<SewingTeamDto>> CreateAsync(CreateSewingTeamRequest dto, CancellationToken ct = default);
    Task<ApiResponse<SewingTeamDto>> UpdateAsync(int id, SewingTeamDto dto, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default);
}

public class SewingTeamClient : ISewingTeamClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _options;

    public SewingTeamClient(HttpClient http, JsonSerializerOptions options)
    {
        _http = http;
        _options = options;
    }

    public async Task<ApiResponse<List<SewingTeamDto>>> GetAllTeamsAsync(CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<ApiResponse<List<SewingTeamDto>>>("api/SewingTeams", _options, ct)
            ?? ApiResponse<List<SewingTeamDto>>.FailureResult("Erro ao carregar equipes.");
    }

    public async Task<ApiResponse<SewingTeamDto>> CreateAsync(CreateSewingTeamRequest dto, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/SewingTeams", dto, _options, ct);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<SewingTeamDto>>(_options, ct)
            ?? ApiResponse<SewingTeamDto>.FailureResult("Erro ao criar equipe.");
    }

    public async Task<ApiResponse<SewingTeamDto>> UpdateAsync(int id, SewingTeamDto dto, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync($"api/SewingTeams/{id}", dto, _options, ct);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<SewingTeamDto>>(_options, ct)
            ?? ApiResponse<SewingTeamDto>.FailureResult("Erro ao atualizar equipe.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync($"api/SewingTeams/{id}", ct);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<bool>>(_options, ct)
            ?? ApiResponse<bool>.FailureResult("Erro ao excluir equipe.");
    }
}
