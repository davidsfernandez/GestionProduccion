using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Models.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InternalCode { get; set; } = string.Empty;
    public string FabricType { get; set; } = string.Empty;
    public string MainSku { get; set; } = string.Empty;
    public double AverageProductionTimeMinutes { get; set; }
    public decimal EstimatedSalePrice { get; set; }
}

public class CreateProductDto
{
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

    public double AverageProductionTimeMinutes { get; set; }

    public decimal EstimatedSalePrice { get; set; }
}

public class UpdateProductDto : CreateProductDto
{
    public int Id { get; set; }
}
