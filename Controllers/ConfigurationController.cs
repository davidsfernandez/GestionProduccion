using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionProduccion.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ConfigurationController : ControllerBase
{
    private readonly ISystemConfigurationService _configService;

    public ConfigurationController(ISystemConfigurationService configService)
    {
        _configService = configService;
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<SystemConfigurationDto>> Get()
    {
        var config = await _configService.GetConfigurationAsync();
        return Ok(config);
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<PublicConfigurationDto>> GetPublic()
    {
        var config = await _configService.GetPublicConfigurationAsync();
        return Ok(config);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Save([FromBody] SystemConfigurationDto request)
    {
        try
        {
            await _configService.SaveConfigurationAsync(request);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("logo")]
    [AllowAnonymous]
    public async Task<ActionResult<LogoDto>> GetLogo()
    {
        var logo = await _configService.GetLogoAsync();
        return Ok(new LogoDto { Base64Image = logo });
    }

    [HttpGet("test-exception")]
    [AllowAnonymous]
    public IActionResult TestException()
    {
        throw new System.Exception("Integration Test Exception");
    }
}
