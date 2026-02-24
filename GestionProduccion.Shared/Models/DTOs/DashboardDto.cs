using System;
using System.Collections.Generic;

namespace GestionProduccion.Models.DTOs;

public class DashboardDto
{
    // --- KEY METRICS ---
    public int TotalActiveOrders { get; set; }
    public int CompletedToday { get; set; }
    public double AverageLeadTimeHours { get; set; } // Efficiency Metric
    public decimal CompletionRate { get; set; }

    // --- CHARTS DATA ---
    public List<int> WeeklyVolumeData { get; set; } = new(); // Exact 7 days array
    public List<string> WeeklyLabels { get; set; } = new();
    public Dictionary<string, int> OrdersByStage { get; set; } = new();
    public List<WorkerStatsDto> WorkloadDistribution { get; set; } = new();

    // --- LISTS & ALERTS ---
    public List<ProductionOrderDto> UrgentOrders { get; set; } = new();
    public List<StoppedOperationDto> StoppedOperations { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
    public List<ProductionOrderDto> TodaysOrders { get; set; } = new();

    // --- CONTROL ---
    public DateTime LastUpdated { get; set; }
}

public class StoppedOperationDto
{
    public int Id { get; set; }
    public string LotCode { get; set; } = string.Empty;
    public DateTime EstimatedDeliveryDate { get; set; }
}

public class WorkerStatsDto
{
    public string Name { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int ActiveCount { get; set; }
    public double EfficiencyScore { get; set; } // Mocked or Calc
    public string Color { get; set; } = "#00C899"; // Helper for UI
}

// Keeping these for compatibility with ProductionOrderService internal references if needed, 
// or for the lists above.
public class RecentActivityDto
{
    public int OrderId { get; set; }
    public string LotCode { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class ChartPointDto
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
}
