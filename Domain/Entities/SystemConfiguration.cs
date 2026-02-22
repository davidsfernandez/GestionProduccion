using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionProduccion.Domain.Entities;

public class SystemConfiguration
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Key { get; set; } = string.Empty;

    public string? Value { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// Stores the company logo as a base64 string or file path.
    /// Added to satisfy the Global Configuration (Logo) requirement.
    /// </summary>
    public string? Logo { get; set; }

    // Phase 3: Financial Module
    [Column(TypeName = "decimal(18,2)")]
    public decimal DailyFixedCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OperationalHourlyCost { get; set; }
}
