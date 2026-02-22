using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using System.Security.Claims;
using GestionProduccion.Domain.Entities;

namespace GestionProduccion.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
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
        var ranking = await _taskService.GetPerformanceRankingAsync();
        return Ok(new ApiResponse<List<RankingEntryDto>> { Success = true, Data = ranking });
    }
}
