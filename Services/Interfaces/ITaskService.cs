/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

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
    void ClearRankingCache();
    Task CheckForLeaderChangeAsync(string previousLeaderName);
}

