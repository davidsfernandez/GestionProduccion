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

    public async Task CalculateFinalOrderCostAsync(ProductionOrder order)
    {
        if (order.ActualStartDate == null || order.ActualEndDate == null)
        {
            // Fallback or skip if dates are missing (e.g. legacy data)
            return; 
        }

        // 1. Get Operational Cost
        var hourlyCostStr = await _configRepo.GetValueAsync("OperationalHourlyCost");
        decimal hourlyCost = 0;
        if (decimal.TryParse(hourlyCostStr, out var val)) hourlyCost = val;

        // 2. Calculate Duration
        var duration = order.ActualEndDate.Value - order.ActualStartDate.Value;
        var totalHours = (decimal)duration.TotalHours;

        if (totalHours < 0) totalHours = 0;

        // 3. Calculate Production Cost
        var totalCost = totalHours * hourlyCost;
        order.CalculatedTotalCost = totalCost;

        // 4. Calculate Average Cost Per Piece
        // Assume valid quantity is total quantity for now (Phase 2 constraint 33 says extract valid items, but entity doesn't have defects yet)
        // We use Quantity.
        if (order.Quantity > 0)
        {
            order.AverageCostPerPiece = totalCost / order.Quantity;
        }
        else
        {
            order.AverageCostPerPiece = 0;
        }

        // 5. Calculate Margin
        if (order.ProductId.HasValue)
        {
            var product = await _productRepo.GetByIdAsync(order.ProductId.Value);
            if (product != null && product.EstimatedSalePrice > 0)
            {
                order.EstimatedProfitMargin = ((product.EstimatedSalePrice - order.AverageCostPerPiece) / product.EstimatedSalePrice) * 100;
            }
            else
            {
                order.EstimatedProfitMargin = 0;
            }
        }
    }
}
