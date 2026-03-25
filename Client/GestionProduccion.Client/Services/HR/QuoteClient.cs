using GestionProduccion.Models.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace GestionProduccion.Client.Services.HR;

public interface IQuoteClient
{
    Task<ApiResponse<QuoteDto>> CreateQuoteAsync(CreateQuoteRequest request);
    Task<ApiResponse<List<QuoteDto>>> GetLeadQuotesAsync(int leadId);
    Task<ApiResponse<QuoteDto>> UpdateQuoteStatusAsync(int quoteId, QuoteStatusDto newStatus);
    Task<byte[]> DownloadQuotePdfAsync(int quoteId);
}

public class QuoteClient : IQuoteClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _options;

    public QuoteClient(HttpClient http, JsonSerializerOptions options)
    {
        _http = http;
        _options = options;
    }

    public async Task<ApiResponse<QuoteDto>> CreateQuoteAsync(CreateQuoteRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/HR/quotes", request, _options);
        return await response.Content.ReadFromJsonAsync<ApiResponse<QuoteDto>>(_options)
            ?? ApiResponse<QuoteDto>.FailureResult("Erro ao criar orçamento.");
    }

    public async Task<ApiResponse<List<QuoteDto>>> GetLeadQuotesAsync(int leadId)
    {
        return await _http.GetFromJsonAsync<ApiResponse<List<QuoteDto>>>($"api/HR/leads/{leadId}/quotes", _options)
            ?? ApiResponse<List<QuoteDto>>.FailureResult("Erro ao carregar orçamentos.");
    }

    public async Task<ApiResponse<QuoteDto>> UpdateQuoteStatusAsync(int quoteId, QuoteStatusDto newStatus)
    {
        var response = await _http.PostAsJsonAsync($"api/HR/quotes/{quoteId}/status", new { NewStatus = newStatus }, _options);
        return await response.Content.ReadFromJsonAsync<ApiResponse<QuoteDto>>(_options)
            ?? ApiResponse<QuoteDto>.FailureResult("Erro ao atualizar status.");
    }

    public async Task<byte[]> DownloadQuotePdfAsync(int quoteId)
    {
        var response = await _http.GetAsync($"api/HR/quotes/{quoteId}/pdf");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsByteArrayAsync();
        }
        return Array.Empty<byte>();
    }
}