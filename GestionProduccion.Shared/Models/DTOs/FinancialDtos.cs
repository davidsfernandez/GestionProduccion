/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

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
    public List<TeamRankingDto> TeamRanking { get; set; } = new();
    public List<int> WeeklyVolumeData { get; set; } = new();
    public List<string> WeeklyLabels { get; set; } = new();
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

public class TeamRankingDto
{
    public string TeamName { get; set; } = string.Empty;
    public int TotalProduced { get; set; }
    public double Efficiency { get; set; }
}


