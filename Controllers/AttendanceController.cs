using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestionProduccion.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly ILogger<AttendanceController> _logger;

    public AttendanceController(IAttendanceService attendanceService, ILogger<AttendanceController> logger)
    {
        _attendanceService = attendanceService;
        _logger = logger;
    }

    [HttpGet("status")]
    public async Task<ActionResult<ApiResponse<AttendanceStatusDto>>> GetStatus()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

            var result = await _attendanceService.GetCurrentStatusAsync(userId, HttpContext.RequestAborted);
            return Ok(ApiResponse<AttendanceStatusDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting attendance status");
            return StatusCode(500, ApiResponse<AttendanceStatusDto>.FailureResult("Erro ao consultar status de ponto."));
        }
    }

    [HttpPost("clock-in")]
    public async Task<ActionResult<ApiResponse<AttendanceDto>>> ClockIn([FromQuery] string? note)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

            var result = await _attendanceService.ClockInAsync(userId, note, HttpContext.RequestAborted);
            return Ok(ApiResponse<AttendanceDto>.SuccessResult(result, "Entrada registrada com sucesso."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AttendanceDto>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during clock-in");
            return StatusCode(500, ApiResponse<AttendanceDto>.FailureResult("Erro ao registrar entrada."));
        }
    }

    [HttpPost("clock-out")]
    public async Task<ActionResult<ApiResponse<AttendanceDto>>> ClockOut([FromQuery] string? note)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

            var result = await _attendanceService.ClockOutAsync(userId, note, HttpContext.RequestAborted);
            return Ok(ApiResponse<AttendanceDto>.SuccessResult(result, "Saída registrada com sucesso."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AttendanceDto>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during clock-out");
            return StatusCode(500, ApiResponse<AttendanceDto>.FailureResult("Erro ao registrar saída."));
        }
    }
}