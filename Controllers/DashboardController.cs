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
        var dashboard = await _dashboardService.GetCompleteDashboardAsync(HttpContext.RequestAborted);
        return Ok(new ApiResponse<DashboardCompleteResponse> { Success = true, Data = dashboard });
    }

    [HttpPost("refresh")]
    [Authorize(Roles = "Administrator")]
    public IActionResult RefreshCache()
    {
        _cache.Remove("DashboardComplete");
        return Ok(new ApiResponse<bool> { Success = true, Data = true });
    }
}
