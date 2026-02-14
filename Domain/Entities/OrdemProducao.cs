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

    // NOTE: To ensure uniqueness of 'UniqueCode' at database level,
    // it is recommended to configure a unique index in DbContext:
    // modelBuilder.Entity<ProductionOrder>().HasIndex(po => po.UniqueCode).IsUnique();
    [Required]
    [StringLength(50)]
    public string UniqueCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string ProductDescription { get; set; } = string.Empty;

    [Required]
    public int Quantity { get; set; }

    [Required]
    public ProductionStage CurrentStage { get; set; }

    [Required]
    public ProductionStatus CurrentStatus { get; set; }

    [Required]
    public DateTime CreationDate { get; set; }

    [Required]
    public DateTime ModificationDate { get; set; }

    public DateTime EstimatedDeliveryDate { get; set; }

    [StringLength(100)]
    public string? ClientName { get; set; }

    [StringLength(20)]
    public string? Size { get; set; }

    // Relationship with User (can be null)
    public int? UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User? AssignedUser { get; set; }

    // Navigation property for history
    public virtual ICollection<ProductionHistory> History { get; set; } = new List<ProductionHistory>();
}
