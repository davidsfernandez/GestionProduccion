using GestionProduccion.Domain.Constants;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Hubs;
using GestionProduccion.Services.Interfaces; // For IFinancialCalculatorService
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading;

namespace GestionProduccion.Services.ProductionOrders;

public class ProductionOrderLifecycleService : IProductionOrderLifecycleService
{
    private readonly IProductionOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository; // For RecalculateProductAverageTimeAsync
    private readonly IHubContext<ProductionHub> _hubContext;
    private readonly IHttpContextAccessor _httpContextAccessor; // For GetCurrentUserId (e.g. for history)

    // Secondary services not directly related to Order lifecycle but called by monolith
    private readonly IFinancialCalculatorService _financialCalculator;

    public ProductionOrderLifecycleService(
        IProductionOrderRepository orderRepository,
        IUserRepository userRepository,
        IProductRepository productRepository,
        IHubContext<ProductionHub> hubContext,
        IHttpContextAccessor httpContextAccessor,
        IFinancialCalculatorService financialCalculator)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _hubContext = hubContext;
        _httpContextAccessor = httpContextAccessor;
        _financialCalculator = financialCalculator;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return 1; // Default or anonymous user ID, consider a more robust solution
    }

    public async Task<bool> AssignTaskAsync(int orderId, int userId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || (user.Role != UserRole.Operator && user.Role != UserRole.Workshop)) return false;

        order.UserId = userId;
        await _orderRepository.UpdateAsync(order);

        var currentUserId = GetCurrentUserId();
        await AddHistory(order.Id, order.CurrentStage, order.CurrentStage, order.CurrentStatus, order.CurrentStatus, currentUserId, $"Atribuído a {user.Name}");
        await _orderRepository.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString(), cancellationToken: ct);

        return true;
    }

    public async Task<bool> UpdateStatusAsync(int orderId, ProductionStatus newStatus, string note, int modifiedByUserId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        var user = await _userRepository.GetByIdAsync(modifiedByUserId);
        if (user != null && (user.Role == UserRole.Operator || user.Role == UserRole.Workshop))
        {
            if (order.UserId != modifiedByUserId) throw new UnauthorizedAccessException("Você só pode atualizar o status de ordens atribuídas a você.");
        }

        if (newStatus == ProductionStatus.Completed && order.CurrentStage != ProductionStage.Packaging) return false;
        if (order.CurrentStatus == ProductionStatus.Completed && newStatus != ProductionStatus.Completed) return false;

        var previousStatus = order.CurrentStatus;
        order.CurrentStatus = newStatus;

        // Timestamp Logic
        if (newStatus == ProductionStatus.InProduction && previousStatus != ProductionStatus.InProduction && order.ActualStartDate == null)
        {
            order.ActualStartDate = DateTime.UtcNow;
        }
        else if (newStatus == ProductionStatus.Completed && previousStatus != ProductionStatus.Completed)
        {
            order.ActualEndDate = DateTime.UtcNow;
        }

        await _orderRepository.UpdateAsync(order);

        var completeNote = string.IsNullOrWhiteSpace(note) ? $"Status alterado de {previousStatus} para {newStatus}" : note;
        await AddHistory(order.Id, order.CurrentStage, order.CurrentStage, previousStatus, newStatus, modifiedByUserId, completeNote);

        if (newStatus == ProductionStatus.Completed && previousStatus != ProductionStatus.Completed)
        {
            // Financial Calculation - Delegated to a specific service
            await _financialCalculator.CalculateFinalOrderCostAsync(order);

            // Product average time calculation - consider moving to an event handler or dedicated service
            await RecalculateProductAverageTimeAsync(order.ProductId, order.CreationDate, DateTime.UtcNow);
        }

        await _orderRepository.SaveChangesAsync();
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
        if (user != null && (user.Role == UserRole.Operator || user.Role == UserRole.Workshop))
        {
            if (order.UserId != modifiedByUserId) throw new UnauthorizedAccessException("Você só pode avançar a etapa de ordens atribuídas a você.");
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

        await _orderRepository.UpdateAsync(order);
        await AddHistory(order.Id, previousStage, newStage, order.CurrentStatus, order.CurrentStatus, modifiedByUserId, $"Avançou para {newStage}");
        await _orderRepository.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString(), cancellationToken: ct);

        return true;
    }

    public async Task<bool> ChangeStageAsync(int orderId, ProductionStage newStage, string note, int modifiedByUserId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        if (newStage < order.CurrentStage && string.IsNullOrWhiteSpace(note))
            throw new InvalidOperationException("É obrigatório fornecer um comentário ao retornar para uma etapa anterior.");

        var user = await _userRepository.GetByIdAsync(modifiedByUserId);
        if (user != null && (user.Role == UserRole.Operator || user.Role == UserRole.Workshop))
        {
            if (order.UserId != modifiedByUserId) throw new UnauthorizedAccessException("Você só pode alterar o estágio de ordens atribuídas a você.");
        }

        var previousStage = order.CurrentStage;
        order.CurrentStage = newStage;
        order.CurrentStatus = ProductionStatus.InProduction;

        await _orderRepository.UpdateAsync(order);
        var actionText = newStage < previousStage ? "Retornou para" : "Alterou para";
        var historyNote = string.IsNullOrWhiteSpace(note) ? $"{actionText} {newStage}" : $"{actionText} {newStage}: {note}";
        
        await AddHistory(order.Id, previousStage, newStage, order.CurrentStatus, order.CurrentStatus, modifiedByUserId, historyNote);
        await _orderRepository.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString(), cancellationToken: ct);

        return true;
    }

    // Private helper for history - could be a separate service later
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
            ModificationDate = DateTime.UtcNow,
            Note = note
        };
        await _orderRepository.AddHistoryAsync(history);
    }

    // Method moved from original service for product average time calculation
    // This could also be a separate service or an event handler
    private async Task RecalculateProductAverageTimeAsync(int productId, DateTime startDate, DateTime endDate)
    {
        var durationMinutes = (endDate - startDate).TotalMinutes;
        if (durationMinutes < 0) durationMinutes = 0;

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return;

        var query = await _orderRepository.GetQueryableAsync();
        var completedOrders = await query
            .AsNoTracking()
            .Where(o => o.ProductId == productId && o.CurrentStatus == ProductionStatus.Completed)
            .Select(o => new { o.CreationDate, o.ModificationDate })
            .ToListAsync();

        double totalMinutes = durationMinutes;
        int count = 1;

        foreach (var order in completedOrders)
        {
            var d = (order.ModificationDate - order.CreationDate).TotalMinutes;
            if (d > 0) { totalMinutes += d; count++; }
        }

        if (count > 0)
        {
            product.AverageProductionTimeMinutes = totalMinutes / count;
            await _productRepository.UpdateAsync(product);
        }
    }
}
