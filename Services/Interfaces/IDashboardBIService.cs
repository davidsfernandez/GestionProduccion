using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Services.Interfaces;

public interface IDashboardBIService
{
    Task<DashboardCompleteResponse> GetCompleteDashboardAsync();
}
