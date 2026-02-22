namespace GestionProduccion.Client.Models.DTOs;

public class DashboardDto
{
    public int TotalActiveOrders { get; set; }
    public int CompletedToday { get; set; }
    public double AverageLeadTimeHours { get; set; }
    public List<int> WeeklyVolumeData { get; set; } = new();
    public List<WorkerStatsDto> WorkloadDistribution { get; set; } = new();
    public Dictionary<string, int> OrdersByStage { get; set; } = new();
    public List<ProductionOrderDto> UrgentOrders { get; set; } = new();
    public List<StoppedOperationDto> StoppedOperations { get; set; } = new();
    public List<ProductionOrderDto> TodaysOrders { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
    public double CompletionRate { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class WorkerStatsDto
{
    public string Name { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int ActiveCount { get; set; }
    public double EfficiencyScore { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class StoppedOperationDto
{
    public int OrderId { get; set; }
    public string UniqueCode { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public double DurationHours { get; set; }
}

public class RecentActivityDto
{
    public int OrderId { get; set; }
    public string UniqueCode { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
