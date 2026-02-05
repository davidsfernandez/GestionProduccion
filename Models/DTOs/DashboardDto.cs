using GestionProduccion.Domain.Entities;

namespace GestionProduccion.Models.DTOs;

public class DashboardDto
{
    public Dictionary<string, int> OperationsByStage { get; set; } = new();
    public List<ProductionOrder> StoppedOperations { get; set; } = new();
    public Dictionary<string, List<ProductionOrder>> OperationsByUser { get; set; } = new();
}