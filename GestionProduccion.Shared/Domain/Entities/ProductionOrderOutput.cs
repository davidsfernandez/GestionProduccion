using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Domain.Entities;

/// <summary>
/// Represents a partial or total production output event for a specific size and stage.
/// This allows tracking granular progress within a production order.
/// </summary>
public class ProductionOrderOutput
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProductionOrderId { get; set; }
    [ForeignKey("ProductionOrderId")]
    public virtual ProductionOrder ProductionOrder { get; set; } = null!;

    [Required]
    public int ProductionOrderSizeId { get; set; }
    [ForeignKey("ProductionOrderSizeId")]
    public virtual ProductionOrderSize ProductionOrderSize { get; set; } = null!;

    [Required]
    public ProductionStage Stage { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    // User who recorded this output
    [Required]
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User ResponsibleUser { get; set; } = null!;

    [StringLength(255)]
    public string? Note { get; set; }
}
