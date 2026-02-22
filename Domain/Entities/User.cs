using System.ComponentModel.DataAnnotations;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Domain.Entities;

/// <summary>
/// Represents a system user.
/// </summary>
public class User
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }

    [Required]
    [StringLength(50)]
    public string PublicId { get; set; } = string.Empty;

    [StringLength(250)]
    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; } = true;

    // EF Core navigation properties
    public virtual ICollection<SewingTeam> Teams { get; set; } = new List<SewingTeam>();
    public virtual ICollection<ProductionOrder> AssignedOrders { get; set; } = new List<ProductionOrder>();
    public virtual ICollection<ProductionHistory> HistoryChanges { get; set; } = new List<ProductionHistory>();
}
