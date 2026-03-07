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

    public ProductionOrderQueryService(
        IProductionOrderRepository orderRepository,
        IUserRepository userRepository,
        IHttpContextAccessor httpContextAccessor,
        IProductionOrderOutputRepository outputRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _outputRepository = outputRepository;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return 1; // Default or anonymous user ID
    }

    public async Task<ProductionOrderDto?> GetProductionOrderByIdAsync(int id, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null) return null;

        var outputs = await _outputRepository.GetByOrderIdAsync(id);
        return MapToDto(order, outputs.ToList());
    }

    public async Task<PaginatedResponseDto<ProductionOrderDto>> ListProductionOrdersAsync(FilterProductionOrderDto? filter, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var query = await _orderRepository.GetQueryableAsync();
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
                                         (po.Product != null && po.Product.Name.ToLower().Contains(term)) ||
                                         (po.ClientName != null && po.ClientName.ToLower().Contains(term)));
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
            Items = ordersList.Select(MapToDto).ToList(),
            TotalItems = totalItems,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var query = await _orderRepository.GetQueryableAsync();

        // Eager load related entities for the report and UI
        var ordersWithRelations = query
            .Include(o => o.AssignedUser)
            .Include(o => o.AssignedTeam)
            .AsNoTracking();

        var totalActiveOrders = ordersWithRelations.Count(o => o.CurrentStatus != ProductionStatus.Completed && o.CurrentStatus != ProductionStatus.Cancelled);
        var completedToday = ordersWithRelations
            .Where(o => o.CurrentStatus == ProductionStatus.Completed && o.CompletedAt >= today)
            .Sum(o => o.Quantity);

        var activeOrdersList = ordersWithRelations
            .Where(o => o.CurrentStatus != ProductionStatus.Completed && o.CurrentStatus != ProductionStatus.Cancelled && o.UserId.HasValue)
            .ToList();

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

        var totalAll = ordersWithRelations.Count();
        var totalComp = ordersWithRelations.Count(o => o.CurrentStatus == ProductionStatus.Completed);
        var rate = totalAll > 0 ? (decimal)totalComp / totalAll * 100 : 0;

        var todaysOrdersList = ordersWithRelations.Where(o => o.CreatedAt >= today || (o.CompletedAt != null && o.CompletedAt >= today)).OrderByDescending(o => o.CreatedAt).ToList();
        
        // Fetch outputs for today's orders to ensure accurate mapping
        var orderIds = todaysOrdersList.Select(o => o.Id).ToList();
        var allOutputs = await _outputRepository.GetByOrderIdAsync(0); // Mock/Load all or filter if possible
        // Better: load specific outputs
        var todaysOutputs = new List<ProductionOrderOutput>();
        foreach(var id in orderIds) {
            todaysOutputs.AddRange(await _outputRepository.GetByOrderIdAsync(id));
        }

        var todaysOrdersDtos = todaysOrdersList.Select(o => MapToDto(o, todaysOutputs.Where(x => x.ProductionOrderId == o.Id).ToList())).ToList();

        double avgTimePerPiece = 0;
        var completedTodayList = todaysOrdersList.Where(o => o.CurrentStatus == ProductionStatus.Completed).ToList();
        if (completedTodayList.Any())
        {
            double totalMinutes = completedTodayList.Sum(o => CalculateEffectiveMinutes(o));
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
            .Where(o => (o.SewingTeamId == user.SewingTeamId || o.UserId == userId) &&
                       (o.CurrentStatus == ProductionStatus.Pending || o.CurrentStatus == ProductionStatus.InProduction))
            .OrderBy(o => o.EstimatedCompletionAt)
            .ToListAsync(ct);

        return orders.Select(o =>
        {
            var dto = MapToDto(o);
            // If the order has a team ID and it matches the user's team, it's a team task.
            // If it's only assigned to the user (no team or different team), it's individual.
            dto.IsTeamTask = o.SewingTeamId.HasValue && o.SewingTeamId == user.SewingTeamId;
            return dto;
        }).ToList();
    }

    // --- Private mapping and utility methods ---
    private ProductionOrderDto MapToDto(ProductionOrder order)
    {
        return MapToDto(order, new List<ProductionOrderOutput>());
    }

    private ProductionOrderDto MapToDto(ProductionOrder order, List<ProductionOrderOutput> outputs)
    {
        return new ProductionOrderDto
        {
            Id = order.Id,
            LotCode = order.LotCode,
            ProductName = order.Product?.Name,
            ProductCode = order.Product?.InternalCode,
            Quantity = order.Quantity,
            ClientName = order.ClientName,
            Size = order.Size,
            CurrentStage = order.CurrentStage.ToString(),
            CurrentStatus = order.CurrentStatus.ToString(),
            CreatedAt = order.CreatedAt,
            EstimatedCompletionAt = order.EstimatedCompletionAt,
            UserId = order.UserId,
            AssignedUserName = order.AssignedUser?.FullName,
            AssignedUserAvatar = order.AssignedUser?.AvatarUrl,
            SewingTeamId = order.SewingTeamId,
            SewingTeamName = order.AssignedTeam?.Name,
            TotalCost = order.TotalCost,
            Sizes = order.Sizes?.Select(s => new ProductionOrderSizeDto
            {
                Id = s.Id,
                ProductionOrderId = s.ProductionOrderId,
                Size = s.Size,
                Quantity = s.Quantity,
                CompletedInCurrentStage = outputs
                    .Where(o => o.ProductionOrderSizeId == s.Id && o.Stage == order.CurrentStage)
                    .Sum(o => o.Quantity)
            }).ToList() ?? new List<ProductionOrderSizeDto>(),
            AverageCostPerPiece = order.AverageCostPerPiece,
            ProfitMargin = order.ProfitMargin,
            StartedAt = order.StartedAt,
            CompletedAt = order.CompletedAt,
            EffectiveMinutes = CalculateEffectiveMinutes(order),
            Product = order.Product != null ? new ProductDto
            {
                Id = order.Product.Id,
                Name = order.Product.Name,
                InternalCode = order.Product.InternalCode,
                FabricType = order.Product.FabricType,
                MainSku = order.Product.MainSku,
                AverageProductionTimeMinutes = order.Product.AverageProductionTimeMinutes
            } : null
        };
    }

    private double CalculateEffectiveMinutes(ProductionOrder order)
    {
        if (order.History == null || !order.History.Any()) return 0;

        var sortedHistory = order.History.OrderBy(h => h.ChangedAt).ToList();
        double totalSeconds = 0;
        DateTime? lastStartTime = null;

        foreach (var entry in sortedHistory)
        {
            if (entry.NewStatus == ProductionStatus.InProduction)
            {
                lastStartTime = entry.ChangedAt;
            }
            else if (entry.PreviousStatus == ProductionStatus.InProduction && lastStartTime != null)
            {
                totalSeconds += (entry.ChangedAt - lastStartTime.Value).TotalSeconds;
                lastStartTime = null;
            }
        }

        if (lastStartTime != null)
        {
            var endPoint = order.CompletedAt ?? DateTime.UtcNow;
            totalSeconds += (endPoint - lastStartTime.Value).TotalSeconds;
        }

        return totalSeconds / 60.0;
    }

    private string GetColorByIndex(int index)
    {
        var colors = new[] { "#00C899", "#3B7DDD", "#fcb92c", "#dc3545", "#151628", "#6f42c1", "#e83e8c" };
        return colors[index % colors.Length];
    }
}


