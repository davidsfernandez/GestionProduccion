namespace GestionProduccion.Client.Models.DTOs;

public class DashboardDto
{
    public Dictionary<string, int> OperationsByStage { get; set; } = new();
    public List<StoppedOperationDto> StoppedOperations { get; set; } = new();
    public List<UserWorkloadDto> WorkloadByUser { get; set; } = new();
    public decimal CompletionRate { get; set; }
    public Dictionary<string, double> AverageStageTime { get; set; } = new();

    // Serona Metrics
    public int TotalProducedUnits { get; set; }
    public double EfficiencyTrend { get; set; }
    public List<ChartPointDto> ProductionVolumeHistory { get; set; } = new();
}

public class ChartPointDto
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
}

public class StoppedOperationDto
{
    public int Id { get; set; }
    public string UniqueCode { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public DateTime EstimatedDeliveryDate { get; set; }
}

public class UserWorkloadDto
{
    public string UserName { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
    public int OperationCount { get; set; }
}
