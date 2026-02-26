using Microsoft.EntityFrameworkCore;
using GestionProduccion.Data;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.SignalR;
using GestionProduccion.Hubs;

namespace GestionProduccion.Services;

public class OperationalTaskService : ITaskService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly IHubContext<ProductionHub> _hubContext;
    private const string RankingCacheKey = "PerformanceRanking";

    public OperationalTaskService(AppDbContext context, IMemoryCache cache, IHubContext<ProductionHub> hubContext)
    {
        _context = context;
        _cache = cache;
        _hubContext = hubContext;
    }

    public void ClearRankingCache()
    {
        _cache.Remove(RankingCacheKey);
    }

    public async Task CheckForLeaderChangeAsync(string previousLeaderName)
    {
        ClearRankingCache();
        var currentRanking = await GetPerformanceRankingAsync();
        var currentLeader = currentRanking.FirstOrDefault();

        if (currentLeader != null && currentLeader.UserName != previousLeaderName)
        {
            // The leader changed! Notify everyone
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", new
            {
                message = $"Novo líder no ranking! {currentLeader.UserName} assumiu o 1º lugar com {currentLeader.CompletedTasks} tarefas concluídas! 🏆",
                timestamp = DateTime.UtcNow,
                type = "LeaderChange"
            });
        }
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
        ClearRankingCache();
        return task;
    }

    public async Task<List<TaskDto>> GetUserTasksAsync(int userId)
    {
        return await _context.OperationalTasks
            .AsNoTracking()
            .Where(t => t.AssignedUserId == userId && t.Status != OpTaskStatus.Completed)
            .OrderBy(t => t.Deadline)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

    public async Task<List<TaskDto>> GetAllTasksAsync()
    {
        return await _context.OperationalTasks
            .AsNoTracking()
            .Include(t => t.AssignedUser)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

    public async Task UpdateTaskStatusAsync(int taskId, OpTaskStatus status)
    {
        var task = await _context.OperationalTasks.FindAsync(taskId);
        if (task != null)
        {
            var oldStatus = task.Status;
            
            // Ranking Check Logic - Get Previous Leader
            string previousLeader = "";
            if (status == OpTaskStatus.Completed && oldStatus != OpTaskStatus.Completed)
            {
                var currentRanking = await GetPerformanceRankingAsync();
                previousLeader = currentRanking.FirstOrDefault()?.UserName ?? "";
            }

            task.Status = status;
            if (status == OpTaskStatus.Completed) task.CompletionDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            if (status == OpTaskStatus.Completed && oldStatus != OpTaskStatus.Completed)
            {
                // Re-check ranking and notify if leader changed
                await CheckForLeaderChangeAsync(previousLeader);
            }
        }
    }

    public async Task CompleteTaskAsync(int taskId)
    {
        await UpdateTaskStatusAsync(taskId, OpTaskStatus.Completed);
    }

    public async Task<List<RankingEntryDto>> GetPerformanceRankingAsync(CancellationToken cancellationToken = default)
    {
        var rawData = await _context.Users
            .AsNoTracking()
            .Where(u => u.Role != Domain.Enums.UserRole.Administrator)
            .Select(u => new
            {
                u.FullName,
                AvatarUrl = u.AvatarUrl,
                CompletedTasksCount = _context.OperationalTasks
                    .Count(t => t.AssignedUserId == u.Id && t.Status == OpTaskStatus.Completed),
                CompletedOrdersCount = _context.ProductionOrders
                    .Count(o => o.UserId == u.Id && o.CurrentStatus == ProductionStatus.Completed)
            })
            .ToListAsync(cancellationToken);

        var result = rawData
            .Select(u => {
                int totalCompleted = u.CompletedTasksCount + u.CompletedOrdersCount;
                return new RankingEntryDto
                {
                    UserName = u.FullName,
                    AvatarUrl = u.AvatarUrl ?? "",
                    CompletedTasks = totalCompleted,
                    Score = totalCompleted * 10.0
                };
            })
            .OrderByDescending(r => r.Score)
            .ThenByDescending(r => r.CompletedTasks)
            .Take(10)
            .ToList();

        return result;
    }

    private static TaskDto MapToDto(OperationalTask t) => new TaskDto
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        AssignedUserName = t.AssignedUser?.FullName ?? "N/A",
        Status = t.Status.ToString(),
        CreationDate = t.CreatedAt,
        Deadline = t.Deadline,
        ProgressPercentage = CalculateProgress(t)
    };

    private static double CalculateProgress(OperationalTask t)
    {
        if (t.Status == OpTaskStatus.Completed) return 100;
        if (t.Deadline == null) return 0;

        var total = (t.Deadline.Value - t.CreatedAt).TotalSeconds;
        var elapsed = (DateTime.UtcNow - t.CreatedAt).TotalSeconds;

        var progress = (elapsed / total) * 100;
        return Math.Clamp(progress, 0, 100);
    }
}
