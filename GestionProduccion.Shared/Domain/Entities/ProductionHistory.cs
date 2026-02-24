using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Domain.Entities;

/// <summary>
/// Records change history for production order stage and status.
/// </summary>
public class ProductionHistory
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProductionOrderId { get; set; }
    [ForeignKey("ProductionOrderId")]
    public virtual ProductionOrder ProductionOrder { get; set; } = null!;

    public ProductionStage? PreviousStage { get; set; }
    
    [Required]
    public ProductionStage NewStage { get; set; }

    public ProductionStatus? PreviousStatus { get; set; }
    
    [Required]
    public ProductionStatus NewStatus { get; set; }

    [Required]
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User ResponsibleUser { get; set; } = null!;

    [Required]
    public DateTime ChangedAt { get; set; }

    [StringLength(500)]
    public string Note { get; set; } = string.Empty;
}
