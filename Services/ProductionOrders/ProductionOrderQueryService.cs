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
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Application.Mapping;
using GestionProduccion.Application.Mappers;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading;

namespace GestionProduccion.Services.ProductionOrders;

public class ProductionOrderQueryService : IProductionOrderQueryService
{
    private readonly IProductionOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IProductionOrderOutputRepository _outputRepository;
    private readonly ISystemConfigurationService _configService;
    private readonly MainMapper _mapper;

    public ProductionOrderQueryService(
        IProductionOrderRepository orderRepository,
        IUserRepository userRepository,
        IHttpContextAccessor httpContextAccessor,
        IProductionOrderOutputRepository outputRepository,
        ISystemConfigurationService configService,
        MainMapper mapper)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _outputRepository = outputRepository;
        _configService = configService;
        _mapper = mapper;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User context is required for this operation.");
    }

    public async Task<ProductionOrderDto?> GetProductionOrderByIdAsync(int id, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null) return null;

        // Note: The ManualMapper handles outputs/sizes if they are loaded in the entity
        // Or we could create a specialized ToDto method if needed.
        return order.ToDto();
    }

    public async Task<PaginatedResponseDto<ProductionOrderDto>> ListProductionOrdersAsync(FilterProductionOrderDto? filter, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var query = await _orderRepository.GetQueryableAsync();

        if (filter == null || !filter.IncludeArchived)
        {
            query = query.Where(o => !o.IsArchived);
        }

        var currentUserId = GetCurrentUserId();
        var currentUser = await _userRepository.GetByIdAsync(currentUserId);

        if (currentUser != null && (currentUser.Role == UserRole.Operational))
        {
            query = query.Where(po => po.UserId == currentUserId);
        }

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(po => po.LotCode.ToLower().Contains(term) || 
                                         (po.Product != null && (po.Product.Name.ToLower().Contains(term) || 
                                                                 po.Product.MainSku.ToLower().Contains(term) || 
                                                                 po.Product.InternalCode.ToLower().Contains(term))) ||
                                         (po.ClientName != null && po.ClientName.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(filter.ClientName))
            {
                var client = filter.ClientName.ToLower();
                query = query.Where(po => po.ClientName != null && po.ClientName.ToLower().Contains(client));
            }

            if (!string.IsNullOrWhiteSpace(filter.Size))
            {
                var size = filter.Size.ToLower();
                query = query.Where(po => (po.Size != null && po.Size.ToLower().Contains(size)) || 
                                         po.Sizes.Any(s => s.Size.ToLower().Contains(size)));
            }

            if (!string.IsNullOrWhiteSpace(filter.CurrentStage) && Enum.TryParse<ProductionStage>(filter.CurrentStage, true, out var stage))
                query = query.Where(po => po.CurrentStage == stage);

            if (!string.IsNullOrWhiteSpace(filter.CurrentStatus) && Enum.TryParse<ProductionStatus>(filter.CurrentStatus, true, out var status))
                query = query.Where(po => po.CurrentStatus == status);

            if (filter.UserId.HasValue && filter.UserId.Value > 0)
                query = query.Where(po => po.UserId == filter.UserId.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(po => po.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(po => po.CreatedAt <= filter.EndDate.Value);
        }

        var totalItems = await query.CountAsync(ct);

        var ordersList = await query
            .AsNoTracking()
            .AsSplitQuery()
            .Include(po => po.Product)
            .Include(po => po.AssignedUser)
            .Include(po => po.AssignedTeam)
            .OrderByDescending(po => po.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PaginatedResponseDto<ProductionOrderDto>
        {
            Items = ordersList.ToDtoList(),
            TotalItems = totalItems,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<TvDashboardDto> GetTvDashboardAsync(CancellationToken ct = default)
    {
        // Brazil time adjustment (-3h)
        var brazilToday = DateTime.UtcNow.AddHours(-3).Date;
        var startUtc = brazilToday.AddHours(3);
        var endUtc = startUtc.AddDays(1);

        var query = await _orderRepository.GetQueryableAsync();

        var ordersWithRelations = query
            .Include(o => o.Product)
            .AsNoTracking();

        var completedToday = await CalculateCompletedTodayAsync(startUtc, endUtc, ct);

        // Daily Goal logic (now dynamic from SystemConfiguration)
        var config = await _configService.GetConfigurationAsync();
        int dailyGoal = config?.DailyGoal ?? 500;
        if (dailyGoal <= 0) dailyGoal = 500;

        double completionRate = dailyGoal > 0 ? (double)completedToday / dailyGoal * 100 : 0;

        var activeOrders = await ordersWithRelations
            .Where(o => o.CurrentStatus != ProductionStatus.Completed && o.CurrentStatus != ProductionStatus.Cancelled)
            .OrderByDescending(o => o.UpdatedAt)
            .Take(15)
            .ToListAsync(ct);

        double avgTimePerPiece = 0;
        var completedList = await ordersWithRelations
            .Where(o => o.CurrentStatus == ProductionStatus.Completed && o.CompletedAt >= startUtc && o.CompletedAt < endUtc)
            .Select(o => new { o.EffectiveMinutes, o.Quantity })
            .ToListAsync(ct);

        if (completedList.Any())
        {
            int totalPieces = completedList.Sum(o => o.Quantity);
            if (totalPieces > 0)
                avgTimePerPiece = completedList.Sum(o => o.EffectiveMinutes) / totalPieces;
        }

        // Get TV Announcement from config (Igor's request)
        var announcement = config?.TvAnnouncement ?? "Foco na meta de hoje! Vamos com tudo! 🚀"; 

        return new TvDashboardDto
        {
            CompletedToday = completedToday,
            DailyGoal = dailyGoal,
            CompletionRate = completionRate,
            AverageTimePerPieceMinutes = avgTimePerPiece,
            ActiveOrders = await ordersWithRelations.CountAsync(o => o.CurrentStatus != ProductionStatus.Completed && o.CurrentStatus != ProductionStatus.Cancelled, ct),
            TvAnnouncement = announcement,
            ProductionItems = activeOrders.Select(o => new TvProductionItemDto
            {
                LotCode = o.LotCode,
                ProductCode = o.Product?.InternalCode ?? "N/A",
                ProductName = o.Product?.Name ?? "N/A",
                Quantity = o.Quantity,
                Stage = o.CurrentStage.ToString(),
                Status = o.CurrentStatus.ToString()
            }).ToList()
        };
    }

    public async Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default)
    {
        // Brazil time adjustment (-3h)
        var brazilToday = DateTime.UtcNow.AddHours(-3).Date;
        var startUtc = brazilToday.AddHours(3);
        var endUtc = startUtc.AddDays(1);

        var query = await _orderRepository.GetQueryableAsync();

        // Eager load related entities for the report and UI
        var ordersWithRelations = query
            .Include(o => o.AssignedUser)
            .Include(o => o.AssignedTeam)
            .Include(o => o.Product)
            .AsNoTracking();

        var totalActiveOrders = await ordersWithRelations.CountAsync(o => o.CurrentStatus != ProductionStatus.Completed && o.CurrentStatus != ProductionStatus.Cancelled, ct);
        
        var completedToday = await CalculateCompletedTodayAsync(startUtc, endUtc, ct);

        var activeOrdersList = await ordersWithRelations
            .Where(o => o.CurrentStatus != ProductionStatus.Completed && o.CurrentStatus != ProductionStatus.Cancelled && o.UserId.HasValue)
            .ToListAsync(ct);

        var workloadDistribution = activeOrdersList
            .GroupBy(o => o.UserId!.Value)
            .Select((g, index) => new WorkerStatsDto
            {
                Name = g.First().AssignedUser?.FullName ?? "Unknown",
                AvatarUrl = g.First().AssignedUser?.AvatarUrl ?? "/img/avatars/avatar.jpg",
                ActiveCount = g.Count(),
                EfficiencyScore = 95.0, // Placeholder
                Color = GetColorByIndex(index)
            })
            .OrderByDescending(w => w.ActiveCount)
            .ToList();

        var historyLogs = await _orderRepository.GetRecentHistoryAsync(10);
        var recentActivities = historyLogs.Select(h => new RecentActivityDto
        {
            OrderId = h.ProductionOrderId,
            LotCode = h.ProductionOrder?.LotCode ?? "N/A",
            ProductSku = h.ProductionOrder?.Product?.MainSku ?? h.ProductionOrder?.Product?.InternalCode ?? "N/A",
            UserName = h.ResponsibleUser?.FullName ?? "System",
            Action = h.Note ?? h.NewStatus.ToString(),
            Date = h.ChangedAt
        }).ToList();

        var ordersByStage = query.Where(o => o.CurrentStatus != ProductionStatus.Completed && o.CurrentStatus != ProductionStatus.Cancelled)
            .ToList() // Client evaluation for Enum grouping if EF fails
            .GroupBy(o => o.CurrentStage)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var totalAll = await ordersWithRelations.CountAsync(ct);
        var totalComp = await ordersWithRelations.CountAsync(o => o.CurrentStatus == ProductionStatus.Completed, ct);
        var rate = totalAll > 0 ? (decimal)totalComp / totalAll * 100 : 0;

        var todaysOrdersList = await ordersWithRelations
            .Include(o => o.Sizes)
            .Include(o => o.History)
            .Where(o => o.CreatedAt >= startUtc || (o.CompletedAt != null && o.CompletedAt >= startUtc && o.CompletedAt < endUtc))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
        
        var todaysOrdersDtos = todaysOrdersList.ToDtoList();

        double avgTimePerPiece = 0;
        if (todaysOrdersDtos.Any(o => o.CurrentStatus == "Completed"))
        {
            var completedTodayList = todaysOrdersDtos.Where(o => o.CurrentStatus == "Completed").ToList();
            double totalMinutes = completedTodayList.Sum(o => o.EffectiveMinutes);
            int totalPieces = completedTodayList.Sum(o => o.Quantity);
            if (totalPieces > 0)
                avgTimePerPiece = totalMinutes / totalPieces;
        }

        return new DashboardDto
        {
            TotalActiveOrders = totalActiveOrders,
            CompletedToday = completedToday,
            AverageLeadTimeHours = 0, // Recalculate or retrieve from elsewhere
            AverageTimePerPieceMinutes = avgTimePerPiece,
            WeeklyVolumeData = new List<int> { 0, 0, 0, 0, 0, 0, 0 }, // Populate properly
            WorkloadDistribution = workloadDistribution,
            OrdersByStage = ordersByStage,
            RecentActivities = recentActivities,
            TodaysOrders = todaysOrdersDtos,
            CompletionRate = rate,
            LastUpdated = DateTime.Now
        };
    }

    public async Task<List<ProductionOrderOutputDto>> GetOutputsByOrderIdAsync(int orderId, CancellationToken ct = default)
    {
        var outputs = await _outputRepository.GetByOrderIdAsync(orderId);
        return outputs.Select(o => new ProductionOrderOutputDto
        {
            Id = o.Id,
            ProductionOrderId = o.ProductionOrderId,
            ProductionOrderSizeId = o.ProductionOrderSizeId,
            Size = o.ProductionOrderSize?.Size ?? "N/A",
            Quantity = o.Quantity,
            Stage = o.Stage.ToString(),
            CreatedAt = o.CreatedAt,
            UserName = o.ResponsibleUser?.FullName ?? "System"
        }).ToList();
    }

    public async Task<List<ProductionHistoryDto>> GetHistoryByProductionOrderIdAsync(int orderId, CancellationToken ct = default)
    {
        var history = await _orderRepository.GetHistoryByOrderIdAsync(orderId);
        return history.Select(h => new ProductionHistoryDto
        {
            Id = h.Id,
            ProductionOrderId = h.ProductionOrderId,
            PreviousStage = h.PreviousStage?.ToString() ?? "",
            NewStage = h.NewStage.ToString(),
            PreviousStatus = h.PreviousStatus?.ToString() ?? "",
            NewStatus = h.NewStatus.ToString(),
            UserId = h.UserId,
            UserName = h.ResponsibleUser?.FullName ?? "Unknown",
            ChangedAt = h.ChangedAt,
            Note = h.Note ?? ""
        }).ToList();
    }

    public async Task<List<ProductionOrderDto>> GetTeamProductionOrdersAsync(int userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return new List<ProductionOrderDto>();
        }

        var query = await _orderRepository.GetQueryableAsync();

        // Condition: Assigned to user's team OR assigned directly to user
        var orders = await query
            .AsNoTracking()
            .Include(o => o.Product)
            .Include(o => o.AssignedUser)
            .Include(o => o.AssignedTeam)
            .Where(o => (o.SewingTeamId == user.SewingTeamId || o.UserId == userId) &&
                       (o.CurrentStatus == ProductionStatus.Pending || o.CurrentStatus == ProductionStatus.InProduction))
            .OrderBy(o => o.EstimatedCompletionAt)
            .ToListAsync(ct);

        return orders.Select(o =>
        {
            var dto = o.ToDto();
            // If the order has a team ID and it matches the user's team, it's a team task.
            // If it's only assigned to the user (no team or different team), it's individual.
            dto.IsTeamTask = o.SewingTeamId.HasValue && o.SewingTeamId == user.SewingTeamId;
            return dto;
        }).ToList();
    }

    private string GetColorByIndex(int index)
    {
        var colors = new[] { "#00C899", "#3B7DDD", "#fcb92c", "#dc3545", "#151628", "#6f42c1", "#e83e8c" };
        return colors[index % colors.Length];
    }

    private async Task<int> CalculateCompletedTodayAsync(DateTime startUtc, DateTime endUtc, CancellationToken ct)
    {
        var outputsTodayQuery = await _outputRepository.GetQueryableAsync();
        var ordersQuery = await _orderRepository.GetQueryableAsync();

        // 1. PRECISION FIX: Only count pieces that reached the FINAL stage (Packaging) today.
        // This ensures items in Cutting/Sewing don't inflate the "Produced Today" KPI.
        var completedFromOutputs = await outputsTodayQuery
            .Where(o => o.CreatedAt >= startUtc && o.CreatedAt < endUtc && o.Stage == ProductionStage.Packaging)
            .SumAsync(o => o.Quantity, ct);

        // 2. Legacy/Direct completion (orders marked as Completed today but without specific Packaging outputs today)
        var orderIdsWithPackagingOutputsToday = await outputsTodayQuery
            .Where(o => o.CreatedAt >= startUtc && o.CreatedAt < endUtc && o.Stage == ProductionStage.Packaging)
            .Select(o => o.ProductionOrderId)
            .Distinct()
            .ToListAsync(ct);

        var completedFromLegacy = await ordersQuery
            .Where(o => o.CurrentStatus == ProductionStatus.Completed && 
                        o.CompletedAt >= startUtc && o.CompletedAt < endUtc && 
                        !orderIdsWithPackagingOutputsToday.Contains(o.Id))
            .SumAsync(o => o.Quantity, ct);

        return completedFromOutputs + completedFromLegacy;
    }
}


