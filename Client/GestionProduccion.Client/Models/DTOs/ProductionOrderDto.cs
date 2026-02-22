namespace GestionProduccion.Client.Models.DTOs;

public class ProductionOrderDto
{
    public int Id { get; set; }
    public string UniqueCode { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? ClientName { get; set; }
    public string? Size { get; set; }
    public string CurrentStage { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public DateTime EstimatedDeliveryDate { get; set; }
    public int? UserId { get; set; }
    public string? AssignedUserName { get; set; }
    
    public decimal CalculatedTotalCost { get; set; }
    public decimal AverageCostPerPiece { get; set; }
    public decimal EstimatedProfitMargin { get; set; }

    public List<ProductionHistoryDto> History { get; set; } = new();
}
