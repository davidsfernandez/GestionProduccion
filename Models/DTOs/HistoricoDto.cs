namespace GestionProduccion.Models.DTOs;

/// <summary>
/// DTO for representing a record in the change history of a Production Order.
/// </summary>
public class HistoryDto
{
    public int Id { get; set; }
    public string? PreviousStage { get; set; }
    public string NewStage { get; set; } = string.Empty;
    public string? PreviousStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime ModificationDate { get; set; }
    public string Note { get; set; } = string.Empty;
}
