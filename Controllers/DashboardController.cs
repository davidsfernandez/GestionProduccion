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
    public async Task<ActionResult<DashboardCompleteResponse>> GetComplete()
    {
        if (!_cache.TryGetValue("DashboardComplete", out DashboardCompleteResponse? dashboard))
        {
            dashboard = await _dashboardService.GetCompleteDashboardAsync();
            
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            _cache.Set("DashboardComplete", dashboard, cacheEntryOptions);
        }

        return Ok(dashboard);
    }
    
    [HttpPost("refresh")]
    [Authorize(Roles = "Administrator")]
    public IActionResult RefreshCache()
    {
        _cache.Remove("DashboardComplete");
        return Ok();
    }
}
