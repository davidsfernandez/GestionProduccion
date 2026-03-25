using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Models.DTOs;

public enum QuoteStatusDto
{
    Draft,
    Sent,
    Approved,
    Rejected,
    Expired
}

public class QuoteDto
{
    public int Id { get; set; }
    public int LeadId { get; set; }
    public string LeadName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiryDate { get; set; }
    public QuoteStatusDto Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public List<QuoteItemDto> Items { get; set; } = new();
}

public class QuoteItemDto
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
    
    // Costing info
    public decimal? EstimatedFabricCost { get; set; }
    public decimal? EstimatedLaborCost { get; set; }
    public decimal? ProfitMarginApplied { get; set; }
}

public class CreateQuoteRequest
{
    public int LeadId { get; set; }
    public List<CreateQuoteItemRequest> Items { get; set; } = new();
    public string? Notes { get; set; }
}

public class CreateQuoteItemRequest
{
    public int? ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}