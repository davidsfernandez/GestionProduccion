using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Controllers;

[Authorize(Roles = "Administrator")]
[ApiController]
[Route("api/[controller]")]
public class BonusCalculationController : ControllerBase
{
    private readonly IBonusCalculationService _bonusService;

    public BonusCalculationController(IBonusCalculationService bonusService)
    {
        _bonusService = bonusService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<BonusReportDto>>> GetReport(int? teamId, int? userId, DateTime startDate, DateTime endDate)
    {
        try
        {
            BonusReportDto report;
            if (userId.HasValue)
            {
                report = await _bonusService.CalculateUserBonusAsync(userId.Value, startDate, endDate);
            }
            else if (teamId.HasValue)
            {
                report = await _bonusService.CalculateTeamBonusAsync(teamId.Value, startDate, endDate);
            }
            else
            {
                return BadRequest(new ApiResponse<BonusReportDto> { Success = false, Message = "Either teamId or userId must be provided." });
            }

            return Ok(new ApiResponse<BonusReportDto> { Success = true, Data = report });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<BonusReportDto> { Success = false, Message = ex.Message });
        }
    }
}
