using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionProduccion.Domain.Entities.CRM;

public enum QuoteStatus
{
    Draft,
    Sent,
    Approved,
    Rejected,
    Expired
}

public class Quote
{
    [Key]
    public int Id { get; set; }

    public int LeadId { get; set; }

    [ForeignKey(nameof(LeadId))]
    public virtual Lead? Lead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddDays(15);

    public QuoteStatus Status { get; set; } = QuoteStatus.Draft;

    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();
}

public class QuoteItem
{
    [Key]
    public int Id { get; set; }

    public int QuoteId { get; set; }

    [ForeignKey(nameof(QuoteId))]
    public virtual Quote? Quote { get; set; }

    public int? ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice => Quantity * UnitPrice;

    // Optional dynamic cost breakdown
    public decimal? EstimatedFabricCost { get; set; }
    public decimal? EstimatedLaborCost { get; set; }
    public decimal? ProfitMarginApplied { get; set; }
}