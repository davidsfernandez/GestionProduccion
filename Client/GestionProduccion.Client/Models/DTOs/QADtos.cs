namespace GestionProduccion.Client.Models.DTOs;

public class QADefectDto
{
    public int Id { get; set; }
    public int ProductionOrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime RegistrationDate { get; set; }
}

public class RankingEntryDto
{
    public string UserName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int CompletedTasks { get; set; }
    public double Score { get; set; }
}
