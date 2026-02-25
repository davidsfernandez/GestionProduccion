using System.ComponentModel.DataAnnotations;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Models.DTOs;

public class ProductionOrderDto
{
    public int Id { get; set; }
    public string LotCode { get; set; } = string.Empty;
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public int Quantity { get; set; }
    public string? ClientName { get; set; }
    public string? Size { get; set; }
    public string CurrentStage { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime EstimatedCompletionAt { get; set; }
    public int? UserId { get; set; }
    public string? AssignedUserName { get; set; }
    public int? SewingTeamId { get; set; }
    public string? SewingTeamName { get; set; }
    public bool IsTeamTask { get; set; }
    public decimal TotalCost { get; set; }
    public decimal AverageCostPerPiece { get; set; }
    public decimal ProfitMargin { get; set; }
    public ProductDto? Product { get; set; }
    public List<ProductionHistoryDto> History { get; set; } = new();
}
