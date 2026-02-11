using System.Collections.Generic;

namespace GestionProduccion.Models.DTOs
{
    public class DashboardDto
    {
        public Dictionary<string, int> OperationsByStage { get; set; } = new();

        public List<ProductionOrderDto> StoppedOperations { get; set; } = new();

        public List<UserWorkloadDto> WorkloadByUser { get; set; } = new();

        public decimal CompletionRate { get; set; }

        public Dictionary<string, double> AverageStageTime { get; set; } = new();

        // Serona / Financial Style Metrics
        public int TotalProducedUnits { get; set; }
        public double EfficiencyTrend { get; set; } // e.g. 2.5 for +2.5%
        public List<ChartPointDto> ProductionVolumeHistory { get; set; } = new();
    }

    public class ChartPointDto
    {
        public string Label { get; set; } = string.Empty; // Date or Category
        public double Value { get; set; }
    }

    public class UserWorkloadDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
    }
}
