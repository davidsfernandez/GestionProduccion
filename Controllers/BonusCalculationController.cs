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
    public async Task<ActionResult<ApiResponse<BonusReportDto>>> GetReport(int teamId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var report = await _bonusService.CalculateTeamBonusAsync(teamId, startDate, endDate);
            return Ok(new ApiResponse<BonusReportDto> { Success = true, Data = report });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<BonusReportDto> { Success = false, Message = ex.Message });
        }
    }
}
