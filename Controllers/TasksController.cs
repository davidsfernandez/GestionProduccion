using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using System.Security.Claims;
using GestionProduccion.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace GestionProduccion.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, IMemoryCache cache, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<List<TaskDto>>>> GetMyTasks()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var tasks = await _taskService.GetUserTasksAsync(userId);
        return Ok(new ApiResponse<List<TaskDto>> { Success = true, Data = tasks });
    }

    [HttpGet]
    [Authorize(Roles = "Administrator,Leader")]
    public async Task<ActionResult<ApiResponse<List<TaskDto>>>> GetAll()
    {
        var tasks = await _taskService.GetAllTasksAsync();
        return Ok(new ApiResponse<List<TaskDto>> { Success = true, Data = tasks });
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,Leader")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> Create(CreateTaskDto dto)
    {
        var task = await _taskService.CreateTaskAsync(dto);
        return Ok(new ApiResponse<TaskDto> { Success = true, Message = "Task created" });
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateStatus(int id, [FromBody] OpTaskStatus status)
    {
        await _taskService.UpdateTaskStatusAsync(id, status);
        return Ok(new ApiResponse<string> { Success = true, Message = "Status updated" });
    }

    [HttpGet("ranking")]
    public async Task<ActionResult<ApiResponse<List<RankingEntryDto>>>> GetRanking()
    {
        try
        {
            var ranking = await _cache.GetOrCreateAsync("PerformanceRanking", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await _taskService.GetPerformanceRankingAsync();
            });

            return Ok(new ApiResponse<List<RankingEntryDto>> { Success = true, Data = ranking ?? new List<RankingEntryDto>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate performance ranking");
            // Return empty list instead of 500 to prevent Dashboard crash
            return Ok(new ApiResponse<List<RankingEntryDto>> { Success = true, Data = new List<RankingEntryDto>() });
        }
    }
}
