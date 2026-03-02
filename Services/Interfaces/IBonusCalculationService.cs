using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Services.Interfaces;

public interface IBonusCalculationService
{
    Task<BonusReportDto> CalculateTeamBonusAsync(int teamId, DateTime startDate, DateTime endDate);
    Task<BonusReportDto> CalculateUserBonusAsync(int userId, DateTime startDate, DateTime endDate);
}
