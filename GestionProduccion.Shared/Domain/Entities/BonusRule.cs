using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionProduccion.Domain.Entities;

public class BonusRule
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public double ProductivityPercentage { get; set; }

    public decimal DeadlineBonusPercentage { get; set; }
    
    public decimal DefectLimitPercentage { get; set; }
    
    public decimal DelayPenaltyPercentage { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BonusAmount { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
