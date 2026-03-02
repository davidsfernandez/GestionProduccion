namespace GestionProduccion.Models.DTOs;

public class BonusRuleDto
{
    public int Id { get; set; }
    public decimal ProductivityPercentage { get; set; }
    public decimal DeadlineBonusPercentage { get; set; }
    public decimal DefectLimitPercentage { get; set; }
    public decimal DelayPenaltyPercentage { get; set; }
    public DateTime LastUpdate { get; set; }
}

public class UpdateBonusRuleDto
{
    public decimal ProductivityPercentage { get; set; }
    public decimal DeadlineBonusPercentage { get; set; }
    public decimal DefectLimitPercentage { get; set; }
    public decimal DelayPenaltyPercentage { get; set; }
}
