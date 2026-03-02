using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Domain.Entities;

public class SewingTeam
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // Navigation property for members (Users)
    public virtual ICollection<User> Members { get; set; } = new List<User>();

    // Navigation property for assigned orders
    public virtual ICollection<ProductionOrder> AssignedOrders { get; set; } = new List<ProductionOrder>();
}
