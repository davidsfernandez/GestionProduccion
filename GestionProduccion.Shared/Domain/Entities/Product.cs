using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionProduccion.Domain.Entities;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string InternalCode { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FabricType { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string MainSku { get; set; } = string.Empty;

    // Phase 2: Memory Production
    public double AverageProductionTimeMinutes { get; set; }

    // Phase 3: Financial Module
    [Column(TypeName = "decimal(18,2)")]
    public decimal EstimatedSalePrice { get; set; }

    public virtual ICollection<ProductSize> Sizes { get; set; } = new List<ProductSize>();
}

public class ProductSize
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(10)]
    public string Size { get; set; } = string.Empty; // P, M, G, GG, etc.

    public int ProductId { get; set; }
    
    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; } = null!;
}
