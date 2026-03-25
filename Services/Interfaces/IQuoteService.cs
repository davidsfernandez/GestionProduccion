using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Services.Interfaces;

public interface IQuoteService
{
    Task<QuoteDto> CreateQuoteAsync(CreateQuoteRequest request, CancellationToken ct = default);
    Task<List<QuoteDto>> GetLeadQuotesAsync(int leadId, CancellationToken ct = default);
    Task<QuoteDto> GetQuoteByIdAsync(int quoteId, CancellationToken ct = default);
    Task<List<QuoteDto>> GetCustomerQuotesAsync(int customerUserId, CancellationToken ct = default);
    Task<QuoteDto> UpdateQuoteStatusAsync(int quoteId, Domain.Entities.CRM.QuoteStatus newStatus, CancellationToken ct = default);
}