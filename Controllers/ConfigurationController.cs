using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionProduccion.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConfigurationController : ControllerBase
{
    private readonly ISystemConfigurationService _configService;

    public ConfigurationController(ISystemConfigurationService configService)
    {
        _configService = configService;
    }

    [HttpGet("logo")]
    [AllowAnonymous]
    public async Task<ActionResult<LogoDto>> GetLogo()
    {
        var logo = await _configService.GetLogoAsync();
        return Ok(new LogoDto { Base64Image = logo });
    }

    [HttpPost("logo")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdateLogo([FromBody] LogoDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Base64Image))
        {
            return BadRequest("Invalid logo data.");
        }

        try
        {
            await _configService.UpdateLogoAsync(request.Base64Image);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("financial")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdateFinancial([FromBody] UpdateFinancialConfigDto request)
    {
        await _configService.UpdateFinancialConfigAsync(request.DailyFixedCost, request.OperationalHourlyCost);
        return Ok();
    }
}
