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

[ApiController]
[Route("api/[controller]")]
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
            return Ok(ApiResponse<SystemConfigurationDto>.SuccessResult(config!));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<SystemConfigurationDto>.FailureResult("Error retrieving configuration", new List<string> { ex.Message }));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ApiResponse<bool>>> Save([FromBody] SystemConfigurationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<bool>.FailureResult("Validation failed"));

        try
        {
            var success = await _configService.SaveConfigurationAsync(dto);
            return Ok(ApiResponse<bool>.SuccessResult(success, "Configuration saved successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.FailureResult("Error saving configuration", new List<string> { ex.Message }));
        }
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<PublicConfigurationDto>> GetPublicBranding()
    {
        try
        {
            var config = await _configService.GetConfigurationAsync();
            return Ok(new PublicConfigurationDto
            {
                CompanyName = config.CompanyName,
                LogoBase64 = config.LogoBase64,
                ThemeName = config.ThemeName
            });
        }
        catch
        {
            return Ok(new PublicConfigurationDto { CompanyName = "Serona ERP" });
        }
    }

    [HttpPost("logo")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ApiResponse<bool>>> UploadLogo([FromBody] LogoDto logoDto)
    {
        if (string.IsNullOrEmpty(logoDto.Base64Image))
            return BadRequest(ApiResponse<bool>.FailureResult("No image data provided"));

        try
        {
            var success = await _configService.UpdateLogoAsync(logoDto.Base64Image);
            return Ok(ApiResponse<bool>.SuccessResult(success, "Logo updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.FailureResult("Error updating logo", new List<string> { ex.Message }));
        }
    }
}
