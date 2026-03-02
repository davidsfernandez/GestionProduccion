using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Models.DTOs;

/// <summary>
/// Represents a size and its quantity within a Production Order (DTO).
/// </summary>
public class ProductionOrderSizeDto
{
    public int Id { get; set; }
    public int ProductionOrderId { get; set; }
    public string Size { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
