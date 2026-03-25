using GestionProduccion.Data;
using GestionProduccion.Domain.Entities.CRM;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestionProduccion.Services;

public class QuoteService : IQuoteService
{
    private readonly AppDbContext _context;

    public QuoteService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<QuoteDto> CreateQuoteAsync(CreateQuoteRequest request, CancellationToken ct = default)
    {
        var quote = new Quote
        {
            LeadId = request.LeadId,
            CreatedAt = DateTime.UtcNow,
            Status = QuoteStatus.Draft,
            Notes = request.Notes,
            Items = request.Items.Select(i => new QuoteItem
            {
                ProductId = i.ProductId,
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        quote.TotalAmount = quote.Items.Sum(i => i.TotalPrice);

        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync(ct);

        return await GetQuoteByIdAsync(quote.Id, ct);
    }

    public async Task<List<QuoteDto>> GetLeadQuotesAsync(int leadId, CancellationToken ct = default)
    {
        return await _context.Quotes
            .Include(q => q.Lead)
            .Include(q => q.Items)
            .Where(q => q.LeadId == leadId)
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => MapToDto(q))
            .ToListAsync(ct);
    }

    public async Task<QuoteDto> GetQuoteByIdAsync(int quoteId, CancellationToken ct = default)
    {
        var quote = await _context.Quotes
            .Include(q => q.Lead)
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.Id == quoteId, ct)
            ?? throw new KeyNotFoundException("Quote not found.");

        return MapToDto(quote);
    }

    public async Task<List<QuoteDto>> GetCustomerQuotesAsync(int customerUserId, CancellationToken ct = default)
    {
        return await _context.Quotes
            .Include(q => q.Lead)
            .Include(q => q.Items)
            .Where(q => q.Lead!.CustomerUserId == customerUserId)
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => MapToDto(q))
            .ToListAsync(ct);
    }

    public async Task<QuoteDto> UpdateQuoteStatusAsync(int quoteId, QuoteStatus newStatus, CancellationToken ct = default)
    {
        var quote = await _context.Quotes.FindAsync(new object[] { quoteId }, ct)
            ?? throw new KeyNotFoundException("Quote not found.");

        quote.Status = newStatus;
        await _context.SaveChangesAsync(ct);

        return await GetQuoteByIdAsync(quoteId, ct);
    }

    private static QuoteDto MapToDto(Quote q)
    {
        return new QuoteDto
        {
            Id = q.Id,
            LeadId = q.LeadId,
            LeadName = q.Lead?.Name ?? "Unknown",
            CreatedAt = q.CreatedAt,
            ExpiryDate = q.ExpiryDate,
            Status = (QuoteStatusDto)q.Status,
            TotalAmount = q.TotalAmount,
            Notes = q.Notes,
            Items = q.Items.Select(i => new QuoteItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                EstimatedFabricCost = i.EstimatedFabricCost,
                EstimatedLaborCost = i.EstimatedLaborCost,
                ProfitMarginApplied = i.ProfitMarginApplied
            }).ToList()
        };
    }
}