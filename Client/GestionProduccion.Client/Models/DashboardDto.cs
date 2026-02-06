namespace GestionProduccion.Client.Models
{
    public class DashboardDto
    {
        public Dictionary<string, int>? OperationsByStage { get; set; } = new();
        public List<StoppedOperationDto>? StoppedOperations { get; set; } = new();
        public List<UserWorkloadDto>? WorkloadByUser { get; set; } = new();
    }

    public class StoppedOperationDto
    {
        public int Id { get; set; }
        public string? UniqueCode { get; set; }
        public string? ProductDescription { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
    }

    public class UserWorkloadDto
    {
        public string? UserName { get; set; }
        public int OperationCount { get; set; }
    }
}
