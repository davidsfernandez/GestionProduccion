using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Services.Interfaces;

public interface IBonusCalculationService
{
    Task<BonusReportDto> CalculateTeamBonusAsync(int teamId, DateTime startDate, DateTime endDate);
}
