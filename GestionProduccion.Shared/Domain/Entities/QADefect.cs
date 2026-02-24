using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionProduccion.Domain.Entities;

public class QADefect
{
    [Key]
    public int Id { get; set; }

    public int ProductionOrderId { get; set; }
    [ForeignKey("ProductionOrderId")]
    public virtual ProductionOrder? ProductionOrder { get; set; }

    [Required]
    [StringLength(200)]
    public string Reason { get; set; } = string.Empty;

    public int Quantity { get; set; }

    [StringLength(500)]
    public string? PhotoUrl { get; set; }

    public int ReportedByUserId { get; set; }
    [ForeignKey("ReportedByUserId")]
    public virtual User? ReportedByUser { get; set; }

    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

    public bool IsResolved { get; set; }
}
