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
            // Force UTC translation to avoid timezone mismatch
            var startUtc = startDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(startDate, DateTimeKind.Utc) : startDate.ToUniversalTime();
            var endUtc = endDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(endDate, DateTimeKind.Utc) : endDate.ToUniversalTime();

            BonusReportDto report;
            if (userId.HasValue)
            {
                report = await _bonusService.CalculateUserBonusAsync(userId.Value, startUtc, endUtc);
            }
            else if (teamId.HasValue)
            {
                report = await _bonusService.CalculateTeamBonusAsync(teamId.Value, startUtc, endUtc);
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

    [HttpGet("pdf")]
    public async Task<IActionResult> GetReportPdf(int? teamId, int? userId, DateTime startDate, DateTime endDate, [FromServices] IReportService reportService = null!)
    {
        try
        {
            var startUtc = startDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(startDate, DateTimeKind.Utc) : startDate.ToUniversalTime();
            var endUtc = endDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(endDate, DateTimeKind.Utc) : endDate.ToUniversalTime();

            BonusReportDto report;
            string mode = "team";

            if (userId.HasValue)
            {
                report = await _bonusService.CalculateUserBonusAsync(userId.Value, startUtc, endUtc);
                mode = "individual";
            }
            else if (teamId.HasValue)
            {
                report = await _bonusService.CalculateTeamBonusAsync(teamId.Value, startUtc, endUtc);
                mode = "team";
            }
            else
            {
                return BadRequest("Either teamId or userId must be provided.");
            }

            var pdfBytes = await reportService.GenerateBonusReportPdfAsync(report, mode);
            var fileName = $"Bonus_{report.TeamName}_{startDate:yyyyMM}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}


