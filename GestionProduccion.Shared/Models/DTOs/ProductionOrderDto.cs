using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Models.DTOs;

/// <summary>
/// DTO for Production Order response with simplified information.
/// </summary>
public class ProductionOrderDto
{
    public int Id { get; set; }
    public string UniqueCode { get; set; } = string.Empty;
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public int Quantity { get; set; }
    public string? ClientName { get; set; }
    public string? Tamanho { get; set; }
    public string CurrentStage { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public DateTime EstimatedDeliveryDate { get; set; }
    public int? UserId { get; set; }
    public string? AssignedUserName { get; set; }
    public int? SewingTeamId { get; set; }
    public string? SewingTeamName { get; set; }
    public decimal CalculatedTotalCost { get; set; }
    public ProductDto? Product { get; set; }
    public List<ProductionHistoryDto> History { get; set; } = new();
}
