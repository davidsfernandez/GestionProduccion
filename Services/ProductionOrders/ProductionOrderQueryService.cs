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
    private readonly IUserRepository _userRepository; // Needed for current user logic
    private readonly IHttpContextAccessor _httpContextAccessor; // Needed for GetCurrentUserId

    public ProductionOrderQueryService(
        IProductionOrderRepository orderRepository,
        IUserRepository userRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
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
        return order == null ? null : MapToDto(order);
    }

    public async Task<List<ProductionOrderDto>> ListProductionOrdersAsync(FilterProductionOrderDto? filter, CancellationToken ct = default)
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

            if (!string.IsNullOrWhiteSpace(filter.ClientName))
                query = query.Where(po => po.ClientName != null && po.ClientName.Contains(filter.ClientName));

            if (!string.IsNullOrWhiteSpace(filter.Size))
                query = query.Where(po => po.Size != null && po.Size.Contains(filter.Size));
        }

        // Optimize: do not track entities for read operations, include necessary relations
        // Use AsSplitQuery to avoid Cartesian explosion when loading multiple related entities
        var ordersList = await query
            .AsNoTracking()
            .AsSplitQuery()
            .Include(po => po.Product)
            .Include(po => po.AssignedUser)
            .Include(po => po.AssignedTeam)
            .OrderByDescending(po => po.CreatedAt)
            .ToListAsync(ct);

        return ordersList.Select(MapToDto).ToList();
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
        var completedToday = ordersWithRelations.Count(o => o.CurrentStatus == ProductionStatus.Completed && o.CompletedAt >= today);

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
        var todaysOrdersDtos = todaysOrdersList.Select(MapToDto).ToList();

        return new DashboardDto
        {
            TotalActiveOrders = totalActiveOrders,
            CompletedToday = completedToday,
            AverageLeadTimeHours = 0, // Recalculate or retrieve from elsewhere
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
            SewingTeamId = order.SewingTeamId,
            SewingTeamName = order.AssignedTeam?.Name,
            TotalCost = order.TotalCost,
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

    private string GetColorByIndex(int index)
    {
        var colors = new[] { "#00C899", "#3B7DDD", "#fcb92c", "#dc3545", "#151628", "#6f42c1", "#e83e8c" };
        return colors[index % colors.Length];
    }
}
