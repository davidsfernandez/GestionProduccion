using System.ComponentModel.DataAnnotations;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Domain.Entities.CRM;

public class Lead
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Message { get; set; }

    public LeadStatus Status { get; set; } = LeadStatus.New;

    public LeadSource Source { get; set; } = LeadSource.Website;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string? CommercialNotes { get; set; }

    public virtual ICollection<LeadHistory> History { get; set; } = new List<LeadHistory>();
}