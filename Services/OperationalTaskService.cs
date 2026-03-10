/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited. 
 * 
 * Proprietary and Confidential.
 */

using Microsoft.EntityFrameworkCore;
using GestionProduccion.Data;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;
using GestionProduccion.Hubs;
using GestionProduccion.Application.Mapping;
using GestionProduccion.Application.Mappers;
using System.Security.Claims;

namespace GestionProduccion.Services;

public class OperationalTaskService : ITaskService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly IHubContext<ProductionHub> _hubContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly MainMapper _mapper;
    private const string RankingCacheKey = "PerformanceRanking";

    public OperationalTaskService(
        AppDbContext context, 
        IMemoryCache cache, 
        IHubContext<ProductionHub> hubContext, 
        IHttpContextAccessor httpContextAccessor,
        MainMapper mapper)
    {
        _context = context;
        _cache = cache;
        _hubContext = hubContext;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId)) return userId;
        
        // Fallback for initial setup: find the first active administrator
        var firstAdmin = _context.Users.AsNoTracking()
            .FirstOrDefault(u => u.IsActive && u.Role == UserRole.Administrator);
            
        if (firstAdmin != null) return firstAdmin.Id;

        throw new UnauthorizedAccessException("User context is required for task audit.");
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
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", new
            {
                message = $"Novo líder no ranking! {currentLeader.UserName} assumiu o 1º lugar com {currentLeader.CompletedTasks} tarefas concluídas! 🏆",
                timestamp = DateTime.UtcNow,
                type = "LeaderChange"
            });
        }
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskDto dto)
    {
        var creatorId = GetCurrentUserId();
        
        var task = new OperationalTask
        {
            Title = dto.Title,
            Description = dto.Description,
            AssignedUserId = dto.AssignedUserId,
            Deadline = dto.Deadline,
            Status = OpTaskStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastModifiedByUserId = creatorId
        };

        _context.OperationalTasks.Add(task);
        await _context.SaveChangesAsync();

        // 1. Log Initial Audit History
        _context.OperationalTaskHistories.Add(new OperationalTaskHistory
        {
            OperationalTaskId = task.Id,
            PreviousStatus = null,
            NewStatus = OpTaskStatus.Pending,
            UserId = creatorId,
            ChangedAt = DateTime.UtcNow,
            Note = "Tarefa criada e delegada originalmente."
        });
        await _context.SaveChangesAsync();

        // 2. Real-Time Notification to Assigned User
        if (task.AssignedUserId.HasValue)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", 
                task.AssignedUserId.Value, 
                "Nova Tarefa Delegada", 
                $"Você recebeu a tarefa: {task.Title}");
        }

        ClearRankingCache();
        return _mapper.ToDto(task);
    }

    public async Task<List<TaskDto>> GetUserTasksAsync(int userId)
    {
        var tasks = await _context.OperationalTasks
            .AsNoTracking()
            .Where(t => t.AssignedUserId == userId && t.Status != OpTaskStatus.Completed)
            .OrderBy(t => t.Deadline)
            .ToListAsync();
            
        return _mapper.ToDtoList(tasks);
    }

    public async Task<List<TaskDto>> GetAllTasksAsync()
    {
        var tasks = await _context.OperationalTasks
            .AsNoTracking()
            .Include(t => t.AssignedUser)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
            
        return _mapper.ToDtoList(tasks);
    }

    public async Task UpdateTaskStatusAsync(int taskId, OpTaskStatus status)
    {
        var task = await _context.OperationalTasks.FindAsync(taskId);
        if (task != null)
        {
            var oldStatus = task.Status;
            if (oldStatus == status) return;

            var userId = GetCurrentUserId();
            string previousLeader = "";
            
            if (status == OpTaskStatus.Completed && oldStatus != OpTaskStatus.Completed)
            {
                var currentRanking = await GetPerformanceRankingAsync();
                previousLeader = currentRanking.FirstOrDefault()?.UserName ?? "";
            }

            // Update Task Audit Fields
            task.Status = status;
            task.UpdatedAt = DateTime.UtcNow;
            task.LastModifiedByUserId = userId;
            if (status == OpTaskStatus.Completed) task.CompletionDate = DateTime.UtcNow;

            // Log History
            _context.OperationalTaskHistories.Add(new OperationalTaskHistory
            {
                OperationalTaskId = taskId,
                PreviousStatus = oldStatus,
                NewStatus = status,
                UserId = userId,
                ChangedAt = DateTime.UtcNow,
                Note = $"Status manual change to {status}"
            });

            await _context.SaveChangesAsync();

            if (status == OpTaskStatus.Completed && oldStatus != OpTaskStatus.Completed)
            {
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
            .Where(u => u.IsActive && u.Role != Domain.Enums.UserRole.Administrator)
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

        return rawData
            .Select(u => {
                double calculatedScore = (u.CompletedOrdersCount * 15.0) + (u.CompletedTasksCount * 5.0);
                return new RankingEntryDto
                {
                    UserName = u.FullName,
                    AvatarUrl = u.AvatarUrl ?? "",
                    CompletedTasks = u.CompletedTasksCount + u.CompletedOrdersCount,
                    CompletedOrders = u.CompletedOrdersCount,
                    AdministrativeTasks = u.CompletedTasksCount,
                    Score = calculatedScore
                };
            })
            .OrderByDescending(r => r.Score)
            .Take(10)
            .ToList();
    }
}
