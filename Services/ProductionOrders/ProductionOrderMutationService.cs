/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using GestionProduccion.Domain.Constants;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Hubs;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Application.Mapping;
using GestionProduccion.Application.Mappers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading;

namespace GestionProduccion.Services.ProductionOrders;

public class ProductionOrderMutationService : IProductionOrderMutationService
{
    private readonly IProductionOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly INotificationService _notificationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDistributedLockService _lockService;
    private readonly MainMapper _mapper;

    // Secondary services not directly related to Order mutation but called by monolith
    private readonly IFinancialCalculatorService _financialCalculator;

    public ProductionOrderMutationService(
        IProductionOrderRepository orderRepository,
        IUserRepository userRepository,
        IProductRepository productRepository,
        INotificationService notificationService,
        IHttpContextAccessor httpContextAccessor,
        IDistributedLockService lockService,
        IFinancialCalculatorService financialCalculator,
        MainMapper mapper)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _notificationService = notificationService;
        _httpContextAccessor = httpContextAccessor;
        _lockService = lockService;
        _financialCalculator = financialCalculator;
        _mapper = mapper;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User is not authenticated or user ID claim is missing.");
    }

    public async Task<ProductionOrderDto> CreateProductionOrderAsync(CreateProductionOrderRequest request, int createdByUserId, CancellationToken ct = default)
    {
        // Validation
        if (request.Quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than 0.");

        if (request.EstimatedCompletionAt <= DateTime.UtcNow)
            throw new InvalidOperationException("Estimated delivery date must be in the future.");

        if (string.IsNullOrWhiteSpace(request.Size) && (request.Sizes == null || !request.Sizes.Any()))
            throw new InvalidOperationException("Size or Size list is required.");

        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product == null)
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found.");

        var lockKey = "LOCK_LOTCODE_GENERATION";
        var locked = await _lockService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10), ct);
        if (!locked) throw new InvalidOperationException("Could not acquire lock for lot code generation. Please try again.");

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
                SewingTeamId = request.SewingTeamId,
                // Initialize with base values from product catalog
                AverageCostPerPiece = 0,
                TotalCost = 0,
                ProfitMargin = 100 // Initial margin before real cost calculation
            };

            // Handle multi-size if provided
            if (request.Sizes != null && request.Sizes.Any())
            {
                order.Quantity = request.Sizes.Sum(s => s.Quantity);
                order.Sizes = request.Sizes.Select(s => new ProductionOrderSize
                {
                    Size = s.Size,
                    Quantity = s.Quantity
                }).ToList();
                
                // Set first size as primary for legacy/compatibility if needed
                order.Size = request.Sizes.First().Size;
            }

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

            await _notificationService.NotifyOrderUpdateAsync(order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString(), ct);

            // Re-fetch to ensure all relations are loaded for DTO mapping
            var createdOrder = await _orderRepository.GetByIdAsync(order.Id);

            return createdOrder!.ToDto();
        }
        finally
        {
            await _lockService.ReleaseLockAsync(lockKey);
        }
    }

    public async Task<ProductionOrderDto> UpdateProductionOrderAsync(UpdateProductionOrderRequest request, int modifiedByUserId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id);
        if (order == null) throw new KeyNotFoundException($"Order with ID {request.Id} not found.");

        var oldInfo = $"Client: {order.ClientName}, Delivery: {order.EstimatedCompletionAt:d}";
        
        // 1. Update basic metadata
        order.ClientName = request.ClientName;
        order.EstimatedCompletionAt = request.EstimatedCompletionAt;
        order.UserId = request.UserId;
        order.SewingTeamId = request.SewingTeamId;
        order.UpdatedAt = DateTime.UtcNow;

        // 2. Handle size/quantity updates (Only if in initial stage)
        if (request.Sizes != null && request.Sizes.Any())
        {
            if (order.CurrentStage != ProductionStage.Cutting)
            {
                throw new InvalidOperationException("Quantities cannot be changed once the order has passed the Cutting stage.");
            }

            // Sync sizes: remove old, add new
            order.Sizes.Clear();
            foreach (var s in request.Sizes)
            {
                order.Sizes.Add(new ProductionOrderSize
                {
                    Size = s.Size,
                    Quantity = s.Quantity
                });
            }
            order.Quantity = request.Sizes.Sum(s => s.Quantity);
            // Update legacy field
            order.Size = request.Sizes.First().Size;
        }

        await _orderRepository.UpdateAsync(order);
        await AddHistory(order.Id, order.CurrentStage, order.CurrentStage, order.CurrentStatus, order.CurrentStatus, modifiedByUserId, $"Order metadata updated. Previous: {oldInfo}");
        await _orderRepository.SaveChangesAsync();

        await _notificationService.NotifyOrderUpdateAsync(order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString(), ct);

        return order.ToDto();
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
}


