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
    }

    public class UserWorkloadDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
    }
}
