using Microsoft.EntityFrameworkCore;
using GestionProduccion.Data;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;

namespace GestionProduccion.Services;

public class OperationalTaskService : ITaskService
{
    private readonly AppDbContext _context;

    public OperationalTaskService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OperationalTask> CreateTaskAsync(CreateTaskDto dto)
    {
        var task = new OperationalTask
        {
            Title = dto.Title,
            Description = dto.Description,
            AssignedUserId = dto.AssignedUserId,
            Deadline = dto.Deadline,
            Status = OpTaskStatus.Pending
        };

        _context.OperationalTasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<List<TaskDto>> GetUserTasksAsync(int userId)
    {
        return await _context.OperationalTasks
            .Where(t => t.AssignedUserId == userId)
            .OrderBy(t => t.Deadline)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

    public async Task<List<TaskDto>> GetAllTasksAsync()
    {
        return await _context.OperationalTasks
            .Include(t => t.AssignedUser)
            .OrderByDescending(t => t.CreationDate)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

    public async Task UpdateTaskStatusAsync(int taskId, OpTaskStatus status)
    {
        var task = await _context.OperationalTasks.FindAsync(taskId);
        if (task != null)
        {
            task.Status = status;
            if (status == OpTaskStatus.Completed) task.CompletionDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<RankingEntryDto>> GetPerformanceRankingAsync()
    {
        // Algorithm: (Completed Tasks / Total) * resolution time efficiency
        // For MVP: Count completed tasks within deadline
        var result = await _context.Users
            .Where(u => u.Role != Domain.Enums.UserRole.Administrator)
            .Select(u => new RankingEntryDto
            {
                UserName = u.Name,
                AvatarUrl = u.AvatarUrl ?? "",
                CompletedTasks = _context.OperationalTasks
                    .Count(t => t.AssignedUserId == u.Id && t.Status == OpTaskStatus.Completed),
                Score = _context.OperationalTasks
                    .Where(t => t.AssignedUserId == u.Id && t.Status == OpTaskStatus.Completed)
                    .Average(t => 100.0) // Placeholder for efficiency calculation
            })
            .OrderByDescending(r => r.CompletedTasks)
            .ToListAsync();

        return result;
    }

    private static TaskDto MapToDto(OperationalTask t) => new TaskDto
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        AssignedUserName = t.AssignedUser?.Name ?? "N/A",
        Status = t.Status.ToString(),
        CreationDate = t.CreationDate,
        Deadline = t.Deadline,
        ProgressPercentage = CalculateProgress(t)
    };

    private static double CalculateProgress(OperationalTask t)
    {
        if (t.Status == OpTaskStatus.Completed) return 100;
        if (t.Deadline == null) return 0;
        
        var total = (t.Deadline.Value - t.CreationDate).TotalSeconds;
        var elapsed = (DateTime.UtcNow - t.CreationDate).TotalSeconds;
        
        var progress = (elapsed / total) * 100;
        return Math.Clamp(progress, 0, 100);
    }
}
