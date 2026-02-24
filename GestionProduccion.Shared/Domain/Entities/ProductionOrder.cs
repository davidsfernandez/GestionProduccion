using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Domain.Entities;

/// <summary>
/// Represents a Production Order (PO).
/// </summary>
public class ProductionOrder
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string LotCode { get; set; } = string.Empty;

    // Foreign Key for Product
    [Required]
    public int ProductId { get; set; }
    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; } = null!;

    [Required]
    public int Quantity { get; set; }

    [Required]
    public ProductionStage CurrentStage { get; set; }

    [Required]
    public ProductionStatus CurrentStatus { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public DateTime EstimatedCompletionAt { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AverageCostPerPiece { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ProfitMargin { get; set; }

    [StringLength(100)]
    public string? ClientName { get; set; }

    [StringLength(20)]
    public string? Size { get; set; }

    // Relationship with User (can be null)
    public int? UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User? AssignedUser { get; set; }

    // Bonus Module: Assign to a team
    public int? SewingTeamId { get; set; }
    [ForeignKey("SewingTeamId")]
    public virtual SewingTeam? AssignedTeam { get; set; }

    // Navigation property for history
    public virtual ICollection<ProductionHistory> History { get; set; } = new List<ProductionHistory>();
}
