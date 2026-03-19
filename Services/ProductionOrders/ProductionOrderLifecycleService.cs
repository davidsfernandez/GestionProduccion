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

public class ProductionOrderLifecycleService : IProductionOrderLifecycleService
{
    private readonly IProductionOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IProductionOrderOutputRepository _outputRepository;
    private readonly INotificationService _notificationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IProductService _productService;
    private readonly ITaskService _taskService;
    private readonly IFinancialCalculatorService _financialCalculator;
    private readonly MainMapper _mapper;

    public ProductionOrderLifecycleService(
        IProductionOrderRepository orderRepository,
        IUserRepository userRepository,
        IProductRepository productRepository,
        IProductionOrderOutputRepository outputRepository,
        INotificationService notificationService,
        IHttpContextAccessor httpContextAccessor,
        IFinancialCalculatorService financialCalculator,
        IProductService productService,
        ITaskService taskService,
        MainMapper mapper)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _outputRepository = outputRepository;
        _notificationService = notificationService;
        _httpContextAccessor = httpContextAccessor;
        _financialCalculator = financialCalculator;
        _productService = productService;
        _taskService = taskService;
        _mapper = mapper;
    }

    public async Task<bool> RegisterPartialOutputAsync(int orderId, Dictionary<int, int> sizeOutputs, int modifiedByUserId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null || sizeOutputs == null || !sizeOutputs.Any()) return false;

        var user = await _userRepository.GetByIdAsync(modifiedByUserId);
        if (user != null && user.Role == UserRole.Operational)
        {
            if (order.UserId != modifiedByUserId) throw new UnauthorizedAccessException("You can only record outputs for orders assigned to you.");
        }

        bool anyRegistered = false;
        foreach (var output in sizeOutputs)
        {
            int sizeId = output.Key;
            int quantity = output.Value;
            if (quantity <= 0) continue;

            var orderSize = order.Sizes.FirstOrDefault(s => s.Id == sizeId);
            if (orderSize == null) continue;

            var existingOutputs = await _outputRepository.GetByOrderIdAsync(orderId);
            int alreadyCompletedInStage = existingOutputs.Where(o => o.ProductionOrderSizeId == sizeId && o.Stage == order.CurrentStage).Sum(o => o.Quantity);

            if (alreadyCompletedInStage + quantity > orderSize.Quantity)
                throw new InvalidOperationException($"Exceeded quantity for {orderSize.Size}.");

            await _outputRepository.AddAsync(new ProductionOrderOutput
            {
                ProductionOrderId = orderId,
                ProductionOrderSizeId = sizeId,
                Stage = order.CurrentStage,
                Quantity = quantity,
                UserId = order.UserId ?? modifiedByUserId,
                CreatedAt = DateTime.UtcNow
            });
            anyRegistered = true;
        }

        if (anyRegistered)
        {
            await _outputRepository.SaveChangesAsync();
            
            // --- PRODUCTION INTELLIGENCE: Update WIP Costs ---
            await _financialCalculator.UpdateIntermediateCostAsync(order);
            await _orderRepository.UpdateAsync(order);

            int totalInStage = await _outputRepository.GetTotalQuantityByOrderAndStageAsync(orderId, order.CurrentStage);
            if (totalInStage >= order.Quantity)
            {
                if (order.CurrentStage != ProductionStage.Packaging) await InternalAdvanceStageAsync(order, modifiedByUserId, ct);
                else await UpdateStatusAsync(orderId, ProductionStatus.Completed, "Auto-completed", modifiedByUserId, ct);
            }
            else
            {
                await AddHistory(order.Id, order.CurrentStage, order.CurrentStage, order.CurrentStatus, order.CurrentStatus, modifiedByUserId, "Partial output");
                await _orderRepository.SaveChangesAsync();
                await _notificationService.NotifyOrderUpdateAsync(order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString(), ct);
            }
        }
        return anyRegistered;
    }

    public async Task<ProductionOrderDto?> AssignTaskAsync(int orderId, int userId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return null;

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        order.UserId = userId;
        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order);
        await AddHistory(order.Id, order.CurrentStage, order.CurrentStage, order.CurrentStatus, order.CurrentStatus, userId, $"Assigned to {user.FullName}");
        await _orderRepository.SaveChangesAsync();
        await _notificationService.NotifyOrderUpdateAsync(order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString(), ct);

        return _mapper.ToDto(order);
    }

    public async Task<ProductionOrderDto?> UpdateStatusAsync(int orderId, ProductionStatus newStatus, string note, int modifiedByUserId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return null;

        var previousStatus = order.CurrentStatus;
        var now = DateTime.UtcNow;

        // --- INTEGRITY SHIELD: Persist Effective Minutes ---
        if (previousStatus == ProductionStatus.InProduction && newStatus != ProductionStatus.InProduction)
        {
            // Calculate time since last 'InProduction' entry in history
            var lastStart = order.History
                .Where(h => h.NewStatus == ProductionStatus.InProduction)
                .OrderByDescending(h => h.ChangedAt)
                .FirstOrDefault();

            if (lastStart != null)
            {
                var duration = now - lastStart.ChangedAt;
                order.EffectiveMinutes += duration.TotalMinutes;
            }
        }

        order.CurrentStatus = newStatus;
        order.UpdatedAt = now;

        if (newStatus == ProductionStatus.InProduction && order.StartedAt == null) order.StartedAt = now;
        else if (newStatus == ProductionStatus.Completed) 
        {
            order.CompletedAt = now;
            // Record remaining outputs for the last stage if not already recorded
            await RecordRemainingOutputsAsync(order, modifiedByUserId);
        }

        await _orderRepository.UpdateAsync(order);
        await AddHistory(order.Id, order.CurrentStage, order.CurrentStage, previousStatus, newStatus, modifiedByUserId, note);

        if (newStatus == ProductionStatus.Completed && previousStatus != ProductionStatus.Completed)
        {
            await _financialCalculator.CalculateFinalOrderCostAsync(order);
            await _productService.RecalculateAverageTimeAsync(order.ProductId, ct);
        }

        await _orderRepository.SaveChangesAsync();
        await _notificationService.NotifyOrderUpdateAsync(order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString(), ct);

        return _mapper.ToDto(order);
    }

    public async Task<BulkUpdateResult> BulkUpdateStatusAsync(List<int> orderIds, ProductionStatus newStatus, string note, int modifiedByUserId, CancellationToken ct = default)
    {
        var result = new BulkUpdateResult();
        foreach (var id in orderIds)
        {
            var updated = await UpdateStatusAsync(id, newStatus, note, modifiedByUserId, ct);
            if (updated != null) result.SuccessCount++;
            else result.FailureCount++;
        }
        return result;
    }

    public async Task<ProductionOrderDto?> AdvanceStageAsync(int orderId, int modifiedByUserId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return null;
        await InternalAdvanceStageAsync(order, modifiedByUserId, ct);
        return _mapper.ToDto(order);
    }

    private async Task InternalAdvanceStageAsync(ProductionOrder order, int modifiedByUserId, CancellationToken ct = default)
    {
        var previousStage = order.CurrentStage;
        var newStage = previousStage switch
        {
            ProductionStage.Cutting => ProductionStage.Sewing,
            ProductionStage.Sewing => ProductionStage.Review,
            ProductionStage.Review => ProductionStage.Packaging,
            _ => previousStage
        };

        // If the calculated new stage is the same as current, do nothing (prevent infinite loop)
        if (newStage == previousStage) return;

        // Double check against DB to ensure another thread hasn't already advanced it
        var latestOrder = await _orderRepository.GetByIdAsync(order.Id);
        if (latestOrder != null && latestOrder.CurrentStage != previousStage) return;

        // Record remaining outputs for the stage we are leaving
        await RecordRemainingOutputsAsync(order, modifiedByUserId);

        order.CurrentStage = newStage;
        order.CurrentStatus = ProductionStatus.InProduction;
        order.UpdatedAt = DateTime.UtcNow;

        // --- NEW BUSINESS RULE: Enter Packaging = Automatically Produced Today ---
        // If the new stage is Packaging, we immediately record all remaining pieces
        // so they count towards the "Produced Today" KPI in TV Mode.
        if (newStage == ProductionStage.Packaging)
        {
            await RecordRemainingOutputsAsync(order, modifiedByUserId);
        }

        await _orderRepository.UpdateAsync(order);
        await AddHistory(order.Id, previousStage, newStage, order.CurrentStatus, order.CurrentStatus, modifiedByUserId, $"Advanced to {newStage}");
        await _orderRepository.SaveChangesAsync();
        await _notificationService.NotifyOrderUpdateAsync(order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString(), ct);
    }

    public async Task<bool> ChangeStageAsync(int orderId, ProductionStage newStage, string note, int modifiedByUserId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        var previousStage = order.CurrentStage;
        order.CurrentStage = newStage;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);
        await AddHistory(order.Id, previousStage, newStage, order.CurrentStatus, order.CurrentStatus, modifiedByUserId, note);
        await _orderRepository.SaveChangesAsync();
        await _notificationService.NotifyOrderUpdateAsync(order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString(), ct);

        return true;
    }

    private async Task RecordRemainingOutputsAsync(ProductionOrder order, int userId)
    {
        var existingOutputs = await _outputRepository.GetByOrderIdAsync(order.Id);
        var alreadyRecorded = existingOutputs.Where(o => o.Stage == order.CurrentStage).Sum(o => o.Quantity);
        var remaining = order.Quantity - alreadyRecorded;

        if (remaining > 0)
        {
            // Distribute remaining quantity across sizes
            foreach (var size in order.Sizes)
            {
                var sizeAlready = existingOutputs.Where(o => o.ProductionOrderSizeId == size.Id && o.Stage == order.CurrentStage).Sum(o => o.Quantity);
                var sizeRemaining = size.Quantity - sizeAlready;
                if (sizeRemaining > 0)
                {
                    await _outputRepository.AddAsync(new ProductionOrderOutput
                    {
                        ProductionOrderId = order.Id,
                        ProductionOrderSizeId = size.Id,
                        Stage = order.CurrentStage,
                        Quantity = sizeRemaining,
                        UserId = order.UserId ?? userId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await _outputRepository.SaveChangesAsync();
        }
    }

    private async Task AddHistory(int productionOrderId, ProductionStage? previousStage, ProductionStage newStage, ProductionStatus? previousStatus, ProductionStatus newStatus, int userId, string note)
    {
        await _orderRepository.AddHistoryAsync(new ProductionHistory
        {
            ProductionOrderId = productionOrderId,
            PreviousStage = previousStage,
            NewStage = newStage,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            UserId = userId,
            ChangedAt = DateTime.UtcNow,
            Note = note
        });
    }
}
