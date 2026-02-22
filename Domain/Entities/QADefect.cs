using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionProduccion.Domain.Entities;

public class QADefect
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProductionOrderId { get; set; }
    [ForeignKey("ProductionOrderId")]
    public virtual ProductionOrder ProductionOrder { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Reason { get; set; } = string.Empty;

    [Required]
    public int Quantity { get; set; }

    public string? PhotoUrl { get; set; }

    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    // Optional: Reference to user who reported it
    public int? ReportedByUserId { get; set; }
    [ForeignKey("ReportedByUserId")]
    public virtual User? ReportedBy { get; set; }
}
