using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Services.Interfaces;

public interface ITaskService
{
    Task<OperationalTask> CreateTaskAsync(CreateTaskDto dto);
    Task<List<TaskDto>> GetUserTasksAsync(int userId);
    Task<List<TaskDto>> GetAllTasksAsync();
    Task UpdateTaskStatusAsync(int taskId, OpTaskStatus status);
    Task CompleteTaskAsync(int taskId);
    Task<List<RankingEntryDto>> GetPerformanceRankingAsync(CancellationToken cancellationToken = default);
}
