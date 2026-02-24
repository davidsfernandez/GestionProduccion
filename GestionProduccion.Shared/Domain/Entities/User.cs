using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Domain.Entities;

public class User
{
    [Key]
    public int Id { get; set; }

    public Guid ExternalId { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public string? AvatarUrl { get; set; }

    // Navigation properties
    public virtual ICollection<ProductionOrder> AssignedOrders { get; set; } = new List<ProductionOrder>();
    public virtual ICollection<ProductionHistory> HistoryChanges { get; set; } = new List<ProductionHistory>();

    // Many-to-Many relationship with SewingTeam
    public virtual ICollection<SewingTeam> Teams { get; set; } = new List<SewingTeam>();
}
