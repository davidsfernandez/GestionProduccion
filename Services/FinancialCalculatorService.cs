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
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Services;

public class FinancialCalculatorService : IFinancialCalculatorService
{
    private readonly ISystemConfigurationRepository _configRepo;
    private readonly IProductRepository _productRepo;

    public FinancialCalculatorService(ISystemConfigurationRepository configRepo, IProductRepository productRepo)
    {
        _configRepo = configRepo;
        _productRepo = productRepo;
    }

    /// <summary>
    /// Calculates the real production cost of an order based on execution time and hourly operational cost.
    /// This follows the core financial logic: (Worked Hours * Hourly Cost) / Quantity.
    /// It accurately excludes pause periods by analyzing production history.
    /// </summary>
    /// <param name="order">The production order to calculate cost for.</param>
    public async Task CalculateFinalOrderCostAsync(ProductionOrder order)
    {
        await UpdateIntermediateCostAsync(order);
    }

    public async Task UpdateIntermediateCostAsync(ProductionOrder order)
    {
        // 1. Calculate current effective hours
        // Priority: explicit completed window, then managed EffectiveMinutes, then history fallback
        bool hasExplicitCompletedWindow = order.StartedAt.HasValue &&
                                         order.CompletedAt.HasValue &&
                                         (order.CurrentStatus == ProductionStatus.Completed || order.CurrentStatus == ProductionStatus.Finished);

        double currentEffectiveHours = hasExplicitCompletedWindow
            ? Math.Max(0, (order.CompletedAt!.Value - order.StartedAt!.Value).TotalHours)
            : order.EffectiveMinutes > 0
                ? order.EffectiveMinutes / 60.0
                : CalculateEffectiveWorkingHours(order);

        // If currently in production, add time elapsed since the last recorded start
        if (order.CurrentStatus == ProductionStatus.InProduction && order.StartedAt.HasValue)
        {
            // Note: We use the most recent history entry to find when this specific "run" started
            var lastStart = order.History?
                .Where(h => h.NewStatus == ProductionStatus.InProduction)
                .OrderByDescending(h => h.ChangedAt)
                .FirstOrDefault();

            var startTime = lastStart?.ChangedAt ?? order.StartedAt.Value;
            var elapsed = DateTime.UtcNow - startTime;
            if (elapsed.TotalHours > 0)
            {
                currentEffectiveHours += elapsed.TotalHours;
            }
        }

        if (hasExplicitCompletedWindow)
        {
            order.EffectiveMinutes = currentEffectiveHours * 60.0;
        }

        // 2. Extract costs from configuration
        var config = await _configRepo.GetByKeyAsync("MainConfig");
        decimal hourlyCost = config?.OperationalHourlyCost ?? 45.0m;

        // 3. Labor + agreed bonus
        decimal totalLaborCost = Math.Round((decimal)currentEffectiveHours * hourlyCost, 2);
        int quantity = order.Quantity > 0 ? order.Quantity : 1;
        decimal bonusCostTotal = Math.Round(order.AppliedBonusPerPiece * quantity, 2);
        decimal totalCost = totalLaborCost + bonusCostTotal;
        order.TotalCost = totalCost;

        // 4. Unit / Real Cost (WIP)
        decimal realCost = Math.Round(totalCost / quantity, 2);
        order.AverageCostPerPiece = realCost;

        // 5. Profit Margin Calculation
        var product = await _productRepo.GetByIdAsync(order.ProductId);
        if (product != null && product.EstimatedSalePrice > 0)
        {
            order.ProfitMargin = ((product.EstimatedSalePrice - order.AverageCostPerPiece) / product.EstimatedSalePrice) * 100;
        }
    }

    private double CalculateEffectiveWorkingHours(ProductionOrder order)
    {
        // 1. If history exists, use the precise interval calculation
        if (order.History != null && order.History.Any())
        {
            var sortedHistory = order.History.OrderBy(h => h.ChangedAt).ToList();
            double totalSeconds = 0;
            DateTime? lastStartTime = null;

            foreach (var entry in sortedHistory)
            {
                // If we transitioned TO InProduction, start the clock
                if (entry.NewStatus == ProductionStatus.InProduction)
                {
                    lastStartTime = entry.ChangedAt;
                }
                // If we transitioned FROM InProduction to something else, stop and add interval
                else if (entry.PreviousStatus == ProductionStatus.InProduction && lastStartTime != null)
                {
                    totalSeconds += (entry.ChangedAt - lastStartTime.Value).TotalSeconds;
                    lastStartTime = null;
                }
            }

            // If it's currently InProduction, add time until now (or Completion date)
            if (lastStartTime != null)
            {
                var endPoint = order.CompletedAt ?? DateTime.UtcNow;
                totalSeconds += (endPoint - lastStartTime.Value).TotalSeconds;
            }

            return totalSeconds / 3600.0;
        }

        // 2. Fallback: If no history but we have StartedAt, use the total duration
        if (order.StartedAt.HasValue)
        {
            var endPoint = order.CompletedAt ?? DateTime.UtcNow;
            var duration = endPoint - order.StartedAt.Value;
            return duration.TotalHours > 0 ? duration.TotalHours : 0;
        }

        // 3. Last fallback: CreatedAt to Completion (for very old or mocked data)
        if (order.CompletedAt.HasValue)
        {
            var duration = order.CompletedAt.Value - order.CreatedAt;
            return duration.TotalHours > 0 ? duration.TotalHours : 0;
        }

        return 0;
    }
}


