using GestionProduccion.Data;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestionProduccion.Services;

public class DashboardBIService : IDashboardBIService
{
    private readonly AppDbContext _context;

    public DashboardBIService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardCompleteResponse> GetCompleteDashboardAsync()
    {
        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

        // 1. Month Production (Valid items from completed orders this month)
        var monthProduction = await _context.ProductionOrders
            .AsNoTracking()
            .Where(o => o.CurrentStatus == ProductionStatus.Completed && o.CompletedAt >= firstDayOfMonth)
            .SumAsync(o => o.Quantity);

        // 2. Average Cost Per Piece (Global this month)
        var monthOrders = await _context.ProductionOrders
            .AsNoTracking()
            .Where(o => o.CurrentStatus == ProductionStatus.Completed && o.CompletedAt >= firstDayOfMonth)
            .Select(o => new { o.AverageCostPerPiece, o.ProfitMargin })
            .ToListAsync();

        decimal avgCost = monthOrders.Any() ? monthOrders.Average(o => o.AverageCostPerPiece) : 0;
        decimal avgMargin = monthOrders.Any() ? monthOrders.Average(o => o.ProfitMargin) : 0;

        // 3. Delayed Orders (Active with EstimatedDate < Now)
        var delayedCount = await _context.ProductionOrders
            .AsNoTracking()
            .Where(o => o.CurrentStatus != ProductionStatus.Completed && o.CurrentStatus != ProductionStatus.Cancelled && o.EstimatedCompletionAt < now)
            .CountAsync();

        // 4. Production by Workshop (Group by User ID -> Role Workshop/Operator)
        // Since we don't have explicit Workshop entity, we group by assigned user who is Operator/Workshop
        var prodByWorkshop = await _context.ProductionOrders
            .AsNoTracking()
            .Where(o => o.CurrentStatus == ProductionStatus.Completed && o.UserId.HasValue)
            .GroupBy(o => o.AssignedUser!.FullName)
            .Select(g => new WorkshopProductionDto
            {
                WorkshopName = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .ToListAsync();

        // 5. Top/Bottom Profitable Models (Based on historical completed orders)
        // We need to group by ProductId
        var productStats = await _context.ProductionOrders
            .AsNoTracking()
            .Where(o => o.CurrentStatus == ProductionStatus.Completed)
            .GroupBy(o => o.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                AvgMargin = g.Average(x => x.ProfitMargin)
            })
            .ToListAsync();

        var productIds = productStats.Select(x => x.ProductId).ToList();
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => new { p.Name, p.MainSku });

        var profitabilityList = productStats
            .Where(p => products.ContainsKey(p.ProductId))
            .Select(p => new ProductProfitabilityDto
            {
                Sku = products[p.ProductId].MainSku,
                Name = products[p.ProductId].Name,
                AverageMargin = p.AvgMargin
            })
            .OrderByDescending(p => p.AverageMargin)
            .ToList();

        // 6. Stalled Stock (No production in last 60 days)
        var sixtyDaysAgo = now.AddDays(-60);
        var activeProductIds = await _context.ProductionOrders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= sixtyDaysAgo)
            .Select(o => o.ProductId)
            .Distinct()
            .ToListAsync();

        var stalledProducts = await _context.Products
            .AsNoTracking()
            .Where(p => !activeProductIds.Contains(p.Id))
            .Select(p => new StalledProductDto
            {
                Sku = p.MainSku,
                Name = p.Name,
                DaysSinceLastProduction = 60 // Placeholder, exact calculation requires max date per product query
            })
            .Take(10)
            .ToListAsync();

        return new DashboardCompleteResponse
        {
            MonthProductionQuantity = monthProduction,
            MonthAverageCostPerPiece = Math.Round(avgCost, 2),
            MonthAverageMargin = Math.Round(avgMargin, 2),
            DelayedOrdersCount = delayedCount,
            ProductionByWorkshop = prodByWorkshop,
            TopProfitableModels = profitabilityList.Take(5).ToList(),
            BottomProfitableModels = profitabilityList.OrderBy(p => p.AverageMargin).Take(5).ToList(),
            StalledStock = stalledProducts
        };
    }
}
