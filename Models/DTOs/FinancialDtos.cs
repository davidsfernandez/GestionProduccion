using System.ComponentModel.DataAnnotations;

namespace GestionProduccion.Models.DTOs;

public class UpdateFinancialConfigDto
{
    [Range(0, double.MaxValue)]
    public decimal DailyFixedCost { get; set; }

    [Range(0, double.MaxValue)]
    public decimal OperationalHourlyCost { get; set; }
}

public class UpdateProductFinancialDto
{
    [Range(0, double.MaxValue)]
    public decimal EstimatedSalePrice { get; set; }
}

public class DashboardCompleteResponse
{
    public int MonthProductionQuantity { get; set; }
    public decimal MonthAverageCostPerPiece { get; set; }
    public decimal MonthAverageMargin { get; set; }
    public int DelayedOrdersCount { get; set; }
    
    public List<WorkshopProductionDto> ProductionByWorkshop { get; set; } = new();
    public List<ProductProfitabilityDto> TopProfitableModels { get; set; } = new();
    public List<ProductProfitabilityDto> BottomProfitableModels { get; set; } = new();
    public List<StalledProductDto> StalledStock { get; set; } = new();
}

public class WorkshopProductionDto
{
    public string WorkshopName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class ProductProfitabilityDto
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal AverageMargin { get; set; }
}

public class StalledProductDto
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DaysSinceLastProduction { get; set; }
}
