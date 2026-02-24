using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionProduccion.Domain.Entities;

public class SystemConfiguration
{
    [Key]
    public int Id { get; set; }

    // Company Information
    [StringLength(100)]
    public string? CompanyName { get; set; }

    [StringLength(20)]
    public string? CompanyTaxId { get; set; } // CNPJ in Brazil

    /// <summary>
    /// Stores the company logo as a base64 string.
    /// </summary>
    public string? LogoBase64 { get; set; }

    // Legacy/Generic fields
    [StringLength(50)]
    public string? Key { get; set; }
    public string? Value { get; set; }
    public string? Description { get; set; }

    // Financial Module
    [Column(TypeName = "decimal(18,2)")]
    public decimal DailyFixedCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OperationalHourlyCost { get; set; }
}
