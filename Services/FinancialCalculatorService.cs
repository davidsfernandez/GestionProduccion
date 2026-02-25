using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Services.Interfaces;

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
    /// </summary>
    /// <param name="order">The production order to calculate cost for.</param>
    public async Task CalculateFinalOrderCostAsync(ProductionOrder order)
    {
        // 1. Ensure end time is set
        if (order.CompletedAt == null)
        {
            order.CompletedAt = DateTime.UtcNow;
        }

        // 2. Step 1 of Algorithm: Calculate duration
        // Fallback to CreatedAt if StartedAt was never marked (safety rule)
        var startTime = order.StartedAt ?? order.CreatedAt;
        var duration = order.CompletedAt.Value - startTime;

        // Ensure total hours is at least 1 to avoid zero or negative costs due to rapid completions
        double totalHours = duration.TotalHours > 0 ? duration.TotalHours : 1.0;

        // 3. Step 2 of Algorithm: Extract costs from configuration
        var config = await _configRepo.GetAsync();
        decimal hourlyCost = config?.OperationalHourlyCost ?? 0m;

        // 4. Step 3 of Algorithm: Labor Cost calculation
        decimal totalLaborCost = Math.Round((decimal)totalHours * hourlyCost, 2);
        order.TotalCost = totalLaborCost;

        // 5. Step 4 of Algorithm: Unit / Real Cost
        // Prevent division by zero
        int quantity = order.Quantity > 0 ? order.Quantity : 1;
        decimal realCost = Math.Round(totalLaborCost / quantity, 2);

        order.AverageCostPerPiece = realCost;

        // 6. Profit Margin Calculation (Business Rule)
        var product = await _productRepo.GetByIdAsync(order.ProductId);
        if (product != null && product.EstimatedSalePrice > 0)
        {
            // Margin = ((SalePrice - UnitCost) / SalePrice) * 100
            order.ProfitMargin = ((product.EstimatedSalePrice - order.AverageCostPerPiece) / product.EstimatedSalePrice) * 100;
        }
        else
        {
            order.ProfitMargin = 0;
        }
    }
}
