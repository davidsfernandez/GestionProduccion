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
        if (order.StartedAt == null || order.CompletedAt == null)
        {
            return; 
        }

        var hourlyCostStr = await _configRepo.GetValueAsync("OperationalHourlyCost");
        decimal hourlyCost = 0;
        if (decimal.TryParse(hourlyCostStr, out var val)) hourlyCost = val;

        var duration = order.CompletedAt.Value - order.StartedAt.Value;
        var totalHours = (decimal)duration.TotalHours;

        if (totalHours < 0) totalHours = 0;

        var totalCost = totalHours * hourlyCost;
        order.TotalCost = totalCost;

        if (order.Quantity > 0)
        {
            order.AverageCostPerPiece = totalCost / order.Quantity;
        }
        else
        {
            order.AverageCostPerPiece = 0;
        }

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
}
