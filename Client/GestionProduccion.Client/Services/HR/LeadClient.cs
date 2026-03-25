using GestionProduccion.Models.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace GestionProduccion.Client.Services.HR;

public interface ILeadClient
{
    Task<ApiResponse<List<LeadDto>>> GetLeadsAsync();
    Task<ApiResponse<LeadDto>> UpdateLeadStatusAsync(int leadId, Domain.Enums.LeadStatus newStatus, string? note = null);
}

public class LeadClient : ILeadClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _options;

    public LeadClient(HttpClient http, JsonSerializerOptions options)
    {
        _http = http;
        _options = options;
    }

    public async Task<ApiResponse<List<LeadDto>>> GetLeadsAsync()
    {
        return await _http.GetFromJsonAsync<ApiResponse<List<LeadDto>>>("api/HR/leads", _options)
            ?? ApiResponse<List<LeadDto>>.FailureResult("Erro ao carregar leads.");
    }

    public async Task<ApiResponse<LeadDto>> UpdateLeadStatusAsync(int leadId, Domain.Enums.LeadStatus newStatus, string? note = null)
    {
        var response = await _http.PostAsJsonAsync($"api/HR/leads/{leadId}/status", new { NewStatus = newStatus, Note = note }, _options);
        return await response.Content.ReadFromJsonAsync<ApiResponse<LeadDto>>(_options)
            ?? ApiResponse<LeadDto>.FailureResult("Erro ao atualizar status do lead.");
    }
}