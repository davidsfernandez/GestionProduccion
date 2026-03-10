using System;

namespace GestionProduccion.Models.DTOs;

public class ProductionOrderOutputDto
{
    public int Id { get; set; }
    public int ProductionOrderId { get; set; }
    public int ProductionOrderSizeId { get; set; }
    public string Size { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Stage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Note { get; set; }
    public string UserName { get; set; } = string.Empty;
}
