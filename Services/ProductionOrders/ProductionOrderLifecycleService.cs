using GestionProduccion.Domain.Constants;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Hubs;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading;

namespace GestionProduccion.Services.ProductionOrders;

public class ProductionOrderLifecycleService : IProductionOrderLifecycleService
{
    private readonly IProductionOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IHubContext<ProductionHub> _hubContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IProductService _productService;
    private readonly ITaskService _taskService;

    private readonly IFinancialCalculatorService _financialCalculator;

    public ProductionOrderLifecycleService(
        IProductionOrderRepository orderRepository,
        IUserRepository userRepository,
        IProductRepository productRepository,
        IHubContext<ProductionHub> hubContext,
        IHttpContextAccessor httpContextAccessor,
        IFinancialCalculatorService financialCalculator,
        IProductService productService,
        ITaskService taskService)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _hubContext = hubContext;
        _httpContextAccessor = httpContextAccessor;
        _financialCalculator = financialCalculator;
        _productService = productService;
        _taskService = taskService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return 1;
    }

    public async Task<bool> AssignTaskAsync(int orderId, int userId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || (user.Role != UserRole.Operational)) return false;

        order.UserId = userId;
        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order);

        var currentUserId = GetCurrentUserId();
        await AddHistory(order.Id, order.CurrentStage, order.CurrentStage, order.CurrentStatus, order.CurrentStatus, currentUserId, $"Assigned to {user.FullName}");
        await _orderRepository.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString(), cancellationToken: ct);

        return true;
    }

    public async Task<bool> UpdateStatusAsync(int orderId, ProductionStatus newStatus, string note, int modifiedByUserId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        var user = await _userRepository.GetByIdAsync(modifiedByUserId);
        if (user != null && (user.Role == UserRole.Operational))
        {
            if (order.UserId != modifiedByUserId) throw new UnauthorizedAccessException("You can only update status of orders assigned to you.");
        }

        if (newStatus == ProductionStatus.Completed && order.CurrentStage != ProductionStage.Packaging) return false;
        if (order.CurrentStatus == ProductionStatus.Completed && newStatus != ProductionStatus.Completed) return false;

        var previousStatus = order.CurrentStatus;
        
        // Ranking Check Logic - Get Previous Leader
        string previousLeader = "";
        if (newStatus == ProductionStatus.Completed && previousStatus != ProductionStatus.Completed)
        {
            var currentRanking = await _taskService.GetPerformanceRankingAsync();
            previousLeader = currentRanking.FirstOrDefault()?.UserName ?? "";
        }

        order.CurrentStatus = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        if (newStatus == ProductionStatus.InProduction && previousStatus != ProductionStatus.InProduction && order.StartedAt == null)
        {
            order.StartedAt = DateTime.UtcNow;
        }
        else if (newStatus == ProductionStatus.Completed && previousStatus != ProductionStatus.Completed)
        {
            order.CompletedAt = DateTime.UtcNow;
        }

        await _orderRepository.UpdateAsync(order);

        var completeNote = string.IsNullOrWhiteSpace(note) ? $"Status changed from {previousStatus} to {newStatus}" : note;
        await AddHistory(order.Id, order.CurrentStage, order.CurrentStage, previousStatus, newStatus, modifiedByUserId, completeNote);

        if (newStatus == ProductionStatus.Completed && previousStatus != ProductionStatus.Completed)
        {
            await _financialCalculator.CalculateFinalOrderCostAsync(order);
            await _orderRepository.UpdateAsync(order); // Persistir costos calculados
            await _productService.RecalculateAverageTimeAsync(order.ProductId, ct);
        }

        await _orderRepository.SaveChangesAsync();
        
        // RE-CHECK RANKING AFTER SAVING TO DB
        if (newStatus == ProductionStatus.Completed && previousStatus != ProductionStatus.Completed)
        {
            await _taskService.CheckForLeaderChangeAsync(previousLeader);
        }

        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString(), cancellationToken: ct);

        return true;
    }

    public async Task<BulkUpdateResult> BulkUpdateStatusAsync(List<int> orderIds, ProductionStatus newStatus, string note, int modifiedByUserId, CancellationToken ct = default)
    {
        var result = new BulkUpdateResult();
        foreach (var id in orderIds)
        {
            try
            {
                if (await UpdateStatusAsync(id, newStatus, note, modifiedByUserId, ct)) result.SuccessCount++;
                else { result.FailureCount++; result.Errors.Add($"Order {id}: Update failed."); }
            }
            catch (Exception ex) { result.FailureCount++; result.Errors.Add($"Order {id}: {ex.Message}"); }
        }
        return result;
    }

    public async Task<bool> AdvanceStageAsync(int orderId, int modifiedByUserId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        var user = await _userRepository.GetByIdAsync(modifiedByUserId);
        if (user != null && (user.Role == UserRole.Operational))
        {
            if (order.UserId != modifiedByUserId) throw new UnauthorizedAccessException("You can only advance stage of orders assigned to you.");
        }

        if (order.CurrentStatus == ProductionStatus.Completed) return false;

        var previousStage = order.CurrentStage;
        var newStage = previousStage switch
        {
            ProductionStage.Cutting => ProductionStage.Sewing,
            ProductionStage.Sewing => ProductionStage.Review,
            ProductionStage.Review => ProductionStage.Packaging,
            _ => throw new InvalidOperationException("Unknown production stage or already final.")
        };

        order.CurrentStage = newStage;
        order.CurrentStatus = ProductionStatus.InProduction;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);
        await AddHistory(order.Id, previousStage, newStage, order.CurrentStatus, order.CurrentStatus, modifiedByUserId, $"Advanced to {newStage}");
        await _orderRepository.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString(), cancellationToken: ct);

        return true;
    }

    public async Task<bool> ChangeStageAsync(int orderId, ProductionStage newStage, string note, int modifiedByUserId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        if (newStage < order.CurrentStage && string.IsNullOrWhiteSpace(note))
            throw new InvalidOperationException("Note is required when moving back to a previous stage.");

        var user = await _userRepository.GetByIdAsync(modifiedByUserId);
        if (user != null && (user.Role == UserRole.Operational))
        {
            if (order.UserId != modifiedByUserId) throw new UnauthorizedAccessException("You can only change stage of orders assigned to you.");
        }

        var previousStage = order.CurrentStage;
        order.CurrentStage = newStage;
        order.CurrentStatus = ProductionStatus.InProduction;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);
        var actionText = newStage < previousStage ? "Moved back to" : "Changed to";
        var historyNote = string.IsNullOrWhiteSpace(note) ? $"{actionText} {newStage}" : $"{actionText} {newStage}: {note}";

        await AddHistory(order.Id, previousStage, newStage, order.CurrentStatus, order.CurrentStatus, modifiedByUserId, historyNote);
        await _orderRepository.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString(), cancellationToken: ct);

        return true;
    }

    private async Task AddHistory(int productionOrderId, ProductionStage? previousStage, ProductionStage newStage, ProductionStatus? previousStatus, ProductionStatus newStatus, int userId, string note)
    {
        var history = new ProductionHistory
        {
            ProductionOrderId = productionOrderId,
            PreviousStage = previousStage,
            NewStage = newStage,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            UserId = userId,
            ChangedAt = DateTime.UtcNow,
            Note = note
        };
        await _orderRepository.AddHistoryAsync(history);
    }
}
