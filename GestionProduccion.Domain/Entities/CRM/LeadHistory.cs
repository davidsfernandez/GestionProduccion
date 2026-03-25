using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Domain.Entities.CRM;

public class LeadHistory
{
    [Key]
    public int Id { get; set; }

    public int LeadId { get; set; }

    [ForeignKey(nameof(LeadId))]
    public virtual Lead? Lead { get; set; }

    public LeadStatus PreviousStatus { get; set; }
    public LeadStatus NewStatus { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public int? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? ResponsibleUser { get; set; }

    public string? Note { get; set; }
}