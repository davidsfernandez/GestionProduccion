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

    // Previous stage can be null for the first history record
    public ProductionStage? PreviousStage { get; set; }
    
    [Required]
    public ProductionStage NewStage { get; set; }

    // Previous status can be null for the first history record
    public ProductionStatus? PreviousStatus { get; set; }
    
    [Required]
    public ProductionStatus NewStatus { get; set; }

    [Required]
    public int UserId { get; set; } // User who made the change
    [ForeignKey("UserId")]
    public virtual User ResponsibleUser { get; set; } = null!;

    [Required]
    public DateTime ModificationDate { get; set; }

    /// <summary>
    /// Field to record detailed observations about the change made.
    /// Useful for auditing and troubleshooting.
    /// </summary>
    [StringLength(500)]
    public string Note { get; set; } = string.Empty;
}
