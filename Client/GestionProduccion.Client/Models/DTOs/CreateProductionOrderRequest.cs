using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Client.Models.DTOs;

public class CreateProductionOrderRequest
{
    [Required(ErrorMessage = "Unique code is required.")]
    [StringLength(50)]
    public string UniqueCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Product is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "A valid Product ID must be provided.")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Quantity is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Estimated delivery date is required.")]
    public DateTime EstimatedDeliveryDate { get; set; }

    [StringLength(100)]
    public string? ClientName { get; set; }

    [StringLength(20)]
    public string? Size { get; set; }

    public int? UserId { get; set; }
}
