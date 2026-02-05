using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Models.DTOs;

/// <summary>
/// DTO for Production Order response with simplified information.
/// </summary>
public class ProductionOrderResponseDto
{
    public int Id { get; set; }
    public string UniqueCode { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string CurrentStage { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public DateTime EstimatedDeliveryDate { get; set; }
    public int? UserId { get; set; }
    public string? AssignedUserName { get; set; }
    public List<HistoryDto> History { get; set; } = new();
}
