namespace GestionProduccion.Client.Models.DTOs;

public class BonusRuleDto
{
    public int Id { get; set; }
    public decimal ProductivityPercentage { get; set; }
    public decimal DeadlineBonusPercentage { get; set; }
    public decimal DefectLimitPercentage { get; set; }
    public decimal DelayPenaltyPercentage { get; set; }
    public DateTime LastUpdate { get; set; }
}

public class BonusReportDto
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public decimal ProductivityPercentage { get; set; }
    public decimal DeadlinePerformance { get; set; }
    public decimal DefectPercentage { get; set; }
    public decimal FinalBonusPercentage { get; set; }
    public decimal TotalAmount { get; set; }
    public int CompletedOrders { get; set; }
    public int OnTimeOrders { get; set; }
    public int TotalProduced { get; set; }
    public int TotalDefects { get; set; }
}
