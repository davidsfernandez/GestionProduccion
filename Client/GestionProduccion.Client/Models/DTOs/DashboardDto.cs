using System;
using System.Collections.Generic;

namespace GestionProduccion.Client.Models.DTOs;

public class DashboardDto
{
    public Dictionary<string, int> OperationsByStage { get; set; } = new();
    public List<StoppedOperationDto> StoppedOperations { get; set; } = new();
    public List<UserWorkloadDto> UserWorkloads { get; set; } = new();
    public decimal CompletionRate { get; set; }
    public Dictionary<string, double> AverageStageTime { get; set; } = new();

    // Serona Metrics
    public int TotalProducedUnits { get; set; }
    public double EfficiencyTrend { get; set; }
    public List<ChartPointDto> ProductionVolumeHistory { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
    public List<ProductionOrderDto> UrgentOrders { get; set; } = new();
}

public class UserWorkloadDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int TaskCount { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class RecentActivityDto
{
    public int OrderId { get; set; }
    public string UniqueCode { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Date { get; set; }
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

