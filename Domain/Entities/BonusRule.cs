using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionProduccion.Domain.Entities;

/// <summary>
/// Represents a dynamic configuration for the bonus system (Singleton in practice).
/// </summary>
public class BonusRule
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal ProductivityPercentage { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal DeadlineBonusPercentage { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal DefectLimitPercentage { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal DelayPenaltyPercentage { get; set; }

    public bool IsActive { get; set; } = true;
    
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
}
