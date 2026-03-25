using GestionProduccion.Models.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace GestionProduccion.Client.Services.HR;

public interface ICustomerPortalClient
{
    Task<ApiResponse<List<ProductionOrderDto>>> GetMyOrdersAsync();
    Task<ApiResponse<ProductionOrderDto>> GetOrderDetailsAsync(int orderId);
    Task<ApiResponse<List<QuoteDto>>> GetMyQuotesAsync();
    Task<ApiResponse<QuoteDto>> ApproveQuoteAsync(int quoteId);
    Task<ApiResponse<LeadDto>> ReorderAsync(int orderId);
}

public class CustomerPortalClient : ICustomerPortalClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _options;

    public CustomerPortalClient(HttpClient http, JsonSerializerOptions options)
    {
        _http = http;
        _options = options;
    }

    public async Task<ApiResponse<List<ProductionOrderDto>>> GetMyOrdersAsync()
    {
        return await _http.GetFromJsonAsync<ApiResponse<List<ProductionOrderDto>>>("api/CustomerPortal/orders", _options)
            ?? ApiResponse<List<ProductionOrderDto>>.FailureResult("Erro ao carregar pedidos.");
    }

    public async Task<ApiResponse<ProductionOrderDto>> GetOrderDetailsAsync(int orderId)
    {
        return await _http.GetFromJsonAsync<ApiResponse<ProductionOrderDto>>($"api/CustomerPortal/orders/{orderId}", _options)
            ?? ApiResponse<ProductionOrderDto>.FailureResult("Erro ao carregar detalhes del pedido.");
    }

    public async Task<ApiResponse<List<QuoteDto>>> GetMyQuotesAsync()
    {
        return await _http.GetFromJsonAsync<ApiResponse<List<QuoteDto>>>("api/CustomerPortal/quotes", _options)
            ?? ApiResponse<List<QuoteDto>>.FailureResult("Erro ao carregar orçamentos.");
    }

    public async Task<ApiResponse<QuoteDto>> ApproveQuoteAsync(int quoteId)
    {
        var response = await _http.PostAsJsonAsync($"api/CustomerPortal/quotes/{quoteId}/approve", new { }, _options);
        return await response.Content.ReadFromJsonAsync<ApiResponse<QuoteDto>>(_options)
            ?? ApiResponse<QuoteDto>.FailureResult("Erro ao aprovar orçamento.");
    }

    public async Task<ApiResponse<LeadDto>> ReorderAsync(int orderId)
    {
        var response = await _http.PostAsJsonAsync($"api/CustomerPortal/orders/{orderId}/reorder", new { }, _options);
        return await response.Content.ReadFromJsonAsync<ApiResponse<LeadDto>>(_options)
            ?? ApiResponse<LeadDto>.FailureResult("Erro ao processar re-pedido.");
    }
}