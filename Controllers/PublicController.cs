using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionProduccion.Controllers;

[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly ILeadService _leadService;
    private readonly ILogger<PublicController> _logger;

    public PublicController(ILeadService leadService, ILogger<PublicController> logger)
    {
        _leadService = leadService;
        _logger = logger;
    }

    [HttpPost("leads")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LeadDto>>> SubmitLead([FromBody] CreateLeadDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<LeadDto>.FailureResult("Dados inválidos."));
            }

            var result = await _leadService.CreateLeadAsync(dto, HttpContext.RequestAborted);
            return Ok(ApiResponse<LeadDto>.SuccessResult(result, "Sua solicitação foi recebida com sucesso."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting lead via public API");
            return StatusCode(500, ApiResponse<LeadDto>.FailureResult("Erro interno ao processar solicitação."));
        }
    }
}