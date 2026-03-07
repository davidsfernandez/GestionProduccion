/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

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
        try
        {
            var config = await _configService.GetConfigurationAsync();
            return Ok(ApiResponse<SystemConfigurationDto>.SuccessResult(config));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<SystemConfigurationDto>.FailureResult("Error retrieving configuration", new List<string> { ex.Message }));
        }
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PublicConfigurationDto>>> GetPublic()
    {
        try
        {
            var config = await _configService.GetPublicConfigurationAsync();
            return Ok(ApiResponse<PublicConfigurationDto>.SuccessResult(config));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PublicConfigurationDto>.FailureResult("Error retrieving public configuration", new List<string> { ex.Message }));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ApiResponse<object>>> Save([FromBody] SystemConfigurationDto request)
    {
        try
        {
            await _configService.SaveConfigurationAsync(request);
            return Ok(ApiResponse<object>.SuccessResult(null, "Configuration saved successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResult("Error saving configuration", new List<string> { ex.Message }));
        }
    }

    [HttpGet("logo")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LogoDto>>> GetLogo()
    {
        try
        {
            var logo = await _configService.GetLogoAsync();
            return Ok(ApiResponse<LogoDto>.SuccessResult(new LogoDto { Base64Image = logo }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<LogoDto>.FailureResult("Error retrieving logo", new List<string> { ex.Message }));
        }
    }
}
