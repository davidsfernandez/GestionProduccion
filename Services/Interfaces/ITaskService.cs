using GestionProduccion.Domain.Entities;
using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Services.Interfaces;

public interface ITaskService
{
    Task<OperationalTask> CreateTaskAsync(CreateTaskDto dto);
    Task<List<TaskDto>> GetUserTasksAsync(int userId);
    Task<List<TaskDto>> GetAllTasksAsync();
    Task UpdateTaskStatusAsync(int taskId, OpTaskStatus status);
    Task<List<RankingEntryDto>> GetPerformanceRankingAsync();
}

public class RankingEntryDto
{
    public string UserName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int CompletedTasks { get; set; }
    public double Score { get; set; }
}
