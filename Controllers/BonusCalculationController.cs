/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

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
                return BadRequest(ApiResponse<BonusReportDto>.FailureResult("Either teamId or userId must be provided."));
            }

            return Ok(ApiResponse<BonusReportDto>.SuccessResult(report));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<BonusReportDto>.FailureResult(ex.Message));
        }
    }
}


