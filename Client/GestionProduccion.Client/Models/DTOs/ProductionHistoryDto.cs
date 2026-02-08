namespace GestionProduccion.Client.Models.DTOs;

public class ProductionHistoryDto
{
    public int Id { get; set; }
    public int ProductionOrderId { get; set; }
    public string PreviousStage { get; set; } = string.Empty;
    public string NewStage { get; set; } = string.Empty;
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime ModificationDate { get; set; }
    public string Note { get; set; } = string.Empty;
}
