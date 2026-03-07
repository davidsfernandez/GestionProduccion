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
using Microsoft.Extensions.Caching.Memory;

namespace GestionProduccion.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardBIService _dashboardService;
    private readonly IMemoryCache _cache;

    public DashboardController(IDashboardBIService dashboardService, IMemoryCache cache)
    {
        _dashboardService = dashboardService;
        _cache = cache;
    }

    [HttpGet("completo")]
    public async Task<ActionResult<ApiResponse<DashboardCompleteResponse>>> GetComplete()
    {
        try
        {
            var dashboard = await _dashboardService.GetCompleteDashboardAsync(HttpContext.RequestAborted);
            return Ok(ApiResponse<DashboardCompleteResponse>.SuccessResult(dashboard));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DashboardCompleteResponse>.FailureResult("Error retrieving BI dashboard", new List<string> { ex.Message }));
        }
    }

    [HttpPost("refresh")]
    [Authorize(Roles = "Administrator")]
    public IActionResult RefreshCache()
    {
        _cache.Remove("DashboardComplete");
        return Ok(ApiResponse<object>.SuccessResult(null, "Cache cleared"));
    }
}
