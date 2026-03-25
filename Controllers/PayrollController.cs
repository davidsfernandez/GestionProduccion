using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestionProduccion.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _payrollService;
    private readonly ILogger<PayrollController> _logger;

    public PayrollController(IPayrollService payrollService, ILogger<PayrollController> logger)
    {
        _payrollService = payrollService;
        _logger = logger;
    }

    [HttpGet("my-slip")]
    public async Task<ActionResult<ApiResponse<PayrollSlipDto>>> GetMyCurrentSlip([FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

            var result = await _payrollService.CalculateMonthlyPayrollAsync(userId, year, month, HttpContext.RequestAborted);
            return Ok(ApiResponse<PayrollSlipDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payroll slip");
            return StatusCode(500, ApiResponse<PayrollSlipDto>.FailureResult("Erro ao gerar extrato de pagamento."));
        }
    }

    [Authorize(Roles = "Administrator,Leader")]
    [HttpGet("employee/{userId}/slip")]
    public async Task<ActionResult<ApiResponse<PayrollSlipDto>>> GetEmployeeSlip(int userId, [FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var result = await _payrollService.CalculateMonthlyPayrollAsync(userId, year, month, HttpContext.RequestAborted);
            return Ok(ApiResponse<PayrollSlipDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee {UserId} payroll slip", userId);
            return StatusCode(500, ApiResponse<PayrollSlipDto>.FailureResult("Erro ao consultar folha de pagamento do colaborador."));
        }
    }
}