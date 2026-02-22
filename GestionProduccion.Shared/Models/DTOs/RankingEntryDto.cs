namespace GestionProduccion.Models.DTOs;

public class RankingEntryDto
{
    public string UserName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int CompletedTasks { get; set; }
    public double Score { get; set; }
}
