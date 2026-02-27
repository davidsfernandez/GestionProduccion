using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

    [Column(TypeName = "LONGTEXT")]
    public string? AvatarBase64 { get; set; }

    // Relationship with SewingTeam (1-N)
    public int? SewingTeamId { get; set; }
    [ForeignKey("SewingTeamId")]
    public virtual SewingTeam? SewingTeam { get; set; }

    // Navigation properties
    public virtual ICollection<ProductionOrder> AssignedOrders { get; set; } = new List<ProductionOrder>();
    public virtual ICollection<ProductionHistory> HistoryChanges { get; set; } = new List<ProductionHistory>();
}
