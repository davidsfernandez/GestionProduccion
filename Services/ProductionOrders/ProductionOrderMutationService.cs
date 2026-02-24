using GestionProduccion.Domain.Constants;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Hubs;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces; // For IFinancialCalculatorService
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore; // Added missing using
using System.Security.Claims;
using System.Threading;

namespace GestionProduccion.Services.ProductionOrders;

public class ProductionOrderMutationService : IProductionOrderMutationService
{
    private readonly IProductionOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository; // For validation/history
    private readonly IProductRepository _productRepository; // For validation
    private readonly IHubContext<ProductionHub> _hubContext; // For notifications
    private readonly IHttpContextAccessor _httpContextAccessor; // For GetCurrentUserId (e.g. for history)
    private static readonly SemaphoreSlim _lotCodeSemaphore = new SemaphoreSlim(1, 1);

    // Secondary services not directly related to Order mutation but called by monolith
    private readonly IFinancialCalculatorService _financialCalculator;

    public ProductionOrderMutationService(
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
        return 1; // Default or anonymous user ID
    }

    public async Task<ProductionOrderDto> CreateProductionOrderAsync(CreateProductionOrderRequest request, int createdByUserId, CancellationToken ct = default)
    {
        // Validation
        if (request.Quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than 0.");

        if (request.EstimatedCompletionAt <= DateTime.UtcNow)
            throw new InvalidOperationException("Estimated delivery date must be in the future.");

        if (string.IsNullOrWhiteSpace(request.Size))
            throw new InvalidOperationException("Size is required.");

        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product == null)
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found.");

        await _lotCodeSemaphore.WaitAsync(ct);
        try
        {
            var today = DateTime.UtcNow;
            var prefix = $"OP-{today:yyyy-MM-dd}-";
            
            // Find the max suffix for today
            var query = await _orderRepository.GetQueryableAsync();
            var todaysCodes = await query
                .Where(o => o.LotCode.StartsWith(prefix))
                .Select(o => o.LotCode)
                .ToListAsync(ct);

            int nextSequence = 1;
            if (todaysCodes.Any())
            {
                var maxSuffix = todaysCodes
                    .Select(c => c.Replace(prefix, ""))
                    .Where(s => int.TryParse(s, out _))
                    .Select(int.Parse)
                    .DefaultIfEmpty(0)
                    .Max();
                
                nextSequence = maxSuffix + 1;
            }

            var lotCode = $"{prefix}{nextSequence}";

            var order = new ProductionOrder
            {
                LotCode = lotCode,
                Quantity = request.Quantity,
                EstimatedCompletionAt = request.EstimatedCompletionAt,
                ClientName = request.ClientName,
                Size = request.Size,
                CurrentStage = ProductionStage.Cutting,
                CurrentStatus = ProductionStatus.InProduction,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = request.UserId,
                ProductId = request.ProductId,
                SewingTeamId = request.SewingTeamId
            };

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            var historyNote = "Production order created";
            if (request.UserId.HasValue)
            {
                var assignedUser = await _userRepository.GetByIdAsync(request.UserId.Value);
                if (assignedUser != null) historyNote += $" and assigned to {assignedUser.FullName}";
            }

            await AddHistory(order.Id, null, order.CurrentStage, null, order.CurrentStatus, createdByUserId, historyNote);
            await _orderRepository.SaveChangesAsync(); // Save history

            await _hubContext.Clients.All.SendAsync("ReceiveUpdate", order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString(), cancellationToken: ct);

            // Re-fetch to ensure all relations are loaded for DTO mapping
            var createdOrder = await _orderRepository.GetByIdAsync(order.Id);

            return MapToDto(createdOrder!);
        }
        finally
        {
            _lotCodeSemaphore.Release();
        }
    }

    public async Task<bool> DeleteProductionOrderAsync(int id, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null) return false;

        // Business Rule: block deletion if the order has passed the initial stage (Cutting) or is completed/finished
        if (order.CurrentStage != ProductionStage.Cutting || 
            order.CurrentStatus == ProductionStatus.Completed || 
            order.CurrentStatus == ProductionStatus.Finished)
        {
            throw new InvalidOperationException($"{ErrorMessages.CannotDeleteByBusinessRules}: {ErrorMessages.OrderAlreadyInProgress}");
        }

        await _orderRepository.DeleteAsync(id);
        await _orderRepository.SaveChangesAsync();
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
            .Select(o => new { o.CreatedAt, o.UpdatedAt })
            .ToListAsync();

        double totalMinutes = durationMinutes;
        int count = 1;

        foreach (var order in completedOrders)
        {
            var d = (order.UpdatedAt - order.CreatedAt).TotalMinutes;
            if (d > 0) { totalMinutes += d; count++; }
        }

        if (count > 0)
        {
            product.AverageProductionTimeMinutes = totalMinutes / count;
            await _productRepository.UpdateAsync(product);
        }
    }
}
