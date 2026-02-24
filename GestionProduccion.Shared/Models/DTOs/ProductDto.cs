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
    [Required(ErrorMessage = "O nome do produto é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome não puede exceder 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "O código interno es obrigatório")]
    [StringLength(50, ErrorMessage = "O código não puede exceder 50 caracteres")]
    public string InternalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "O tipo de tecido es obrigatório")]
    [StringLength(50, ErrorMessage = "O tipo de tecido não puede exceder 50 caracteres")]
    public string FabricType { get; set; } = string.Empty;

    [Required(ErrorMessage = "O SKU principal es obrigatório")]
    [StringLength(50, ErrorMessage = "O SKU não puede exceder 50 caracteres")]
    public string MainSku { get; set; } = string.Empty;

    [Range(0.1, double.MaxValue, ErrorMessage = "O tempo médio deve ser maior que zero")]
    public double AverageProductionTimeMinutes { get; set; }

    [Required(ErrorMessage = "O preço estimado es obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
    public decimal EstimatedSalePrice { get; set; }
}

public class UpdateProductDto : CreateProductDto
{
    public int Id { get; set; }
}
