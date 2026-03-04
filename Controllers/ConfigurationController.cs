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
    public async Task<ActionResult<ApiResponse<SystemConfigurationDto>>> Get()
    {
        var config = await _configService.GetConfigurationAsync();
        return Ok(new ApiResponse<SystemConfigurationDto> { Success = true, Data = config });
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PublicConfigurationDto>>> GetPublic()
    {
        var config = await _configService.GetPublicConfigurationAsync();
        return Ok(new ApiResponse<PublicConfigurationDto> { Success = true, Data = config });
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Save([FromBody] SystemConfigurationDto request)
    {
        try
        {
            await _configService.SaveConfigurationAsync(request);
            return Ok(new ApiResponse<bool> { Success = true, Data = true });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<object?> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("logo")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LogoDto>>> GetLogo()
    {
        var logo = await _configService.GetLogoAsync();
        return Ok(new ApiResponse<LogoDto> { Success = true, Data = new LogoDto { Base64Image = logo } });
    }

    [HttpGet("test-exception")]
    [AllowAnonymous]
    public IActionResult TestException()
    {
        throw new System.Exception("Integration Test Exception");
    }
}
