using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Domain.Entities;

/// <summary>
/// Represents a sewing team in the production floor.
/// </summary>
public class SewingTeam
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Relationships
    public virtual ICollection<User> Members { get; set; } = new List<User>();
    public virtual ICollection<ProductionOrder> AssignedOrders { get; set; } = new List<ProductionOrder>();
}
