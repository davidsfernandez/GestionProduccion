using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Models.DTOs;

public class DashboardDto
{
    public Dictionary<string, int> OpsPorEtapa { get; set; } = new();
    public List<OrdemProducao> OpsParadas { get; set; } = new();
    public Dictionary<string, List<OrdemProducao>> OpsPorUsuario { get; set; } = new();
}
