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
        // 1. Ensure end time is set
        if (order.CompletedAt == null)
        {
            order.CompletedAt = DateTime.UtcNow;
        }

        // 2. Calculate Effective Working Hours from History
        double effectiveHours = CalculateEffectiveWorkingHours(order);

        // Safety rule: Minimum 0.1 hours (6 mins) to avoid near-zero costs if history is missing or corrupted
        if (effectiveHours <= 0) 
        {
            var totalDuration = (order.CompletedAt.Value - (order.StartedAt ?? order.CreatedAt)).TotalHours;
            effectiveHours = totalDuration > 0 ? totalDuration : 0.1;
        }

        // 3. Extract costs from configuration
        var config = await _configRepo.GetAsync();
        decimal hourlyCost = config?.OperationalHourlyCost ?? 45.0m;

        // 4. Labor Cost calculation
        decimal totalLaborCost = Math.Round((decimal)effectiveHours * hourlyCost, 2);
        order.TotalCost = totalLaborCost;

        // 5. Unit / Real Cost
        int quantity = order.Quantity > 0 ? order.Quantity : 1;
        decimal realCost = Math.Round(totalLaborCost / quantity, 2);
        order.AverageCostPerPiece = realCost;

        // 6. Profit Margin Calculation
        var product = await _productRepo.GetByIdAsync(order.ProductId);
        if (product != null && product.EstimatedSalePrice > 0)
        {
            order.ProfitMargin = ((product.EstimatedSalePrice - order.AverageCostPerPiece) / product.EstimatedSalePrice) * 100;
        }
        else
        {
            order.ProfitMargin = 0;
        }
    }

    private double CalculateEffectiveWorkingHours(ProductionOrder order)
    {
        if (order.History == null || !order.History.Any()) return 0;

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
}
