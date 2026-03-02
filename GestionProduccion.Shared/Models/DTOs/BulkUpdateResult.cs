namespace GestionProduccion.Models.DTOs;

public class BulkUpdateResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}
