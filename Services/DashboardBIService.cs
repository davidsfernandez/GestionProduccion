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

    public async Task<DashboardCompleteResponse> GetCompleteDashboardAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
        var ptBr = new System.Globalization.CultureInfo("pt-BR");

        // 1. Month Production (Completed this month)
        var monthProduction = await _context.ProductionOrders
            .AsNoTracking()
            .Where(o => o.CurrentStatus == ProductionStatus.Completed && o.CompletedAt >= firstDayOfMonth)
            .SumAsync(o => o.Quantity, ct);

        // 2. Average Cost & Margin
        var monthOrders = await _context.ProductionOrders
            .AsNoTracking()
            .Where(o => o.CurrentStatus == ProductionStatus.Completed && o.CompletedAt >= firstDayOfMonth)
            .Select(o => new { o.AverageCostPerPiece, o.ProfitMargin })
            .ToListAsync(ct);

        decimal avgCost = monthOrders.Any() ? monthOrders.Average(o => o.AverageCostPerPiece) : 0;
        decimal avgMargin = monthOrders.Any() ? monthOrders.Average(o => o.ProfitMargin) : 0;

        // 3. Delayed Orders
        var delayedCount = await _context.ProductionOrders
            .AsNoTracking()
            .Where(o => (o.CurrentStatus == ProductionStatus.Pending || o.CurrentStatus == ProductionStatus.InProduction)
                        && o.EstimatedCompletionAt < now)
            .CountAsync(ct);

        // 4. Production by Workshop
        var prodByWorkshop = await _context.ProductionOrders
            .AsNoTracking()
            .Where(o => o.CurrentStatus == ProductionStatus.Completed && o.UserId.HasValue)
            .GroupBy(o => o.AssignedUser!.FullName)
            .Select(g => new WorkshopProductionDto
            {
                WorkshopName = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .ToListAsync(ct);

        // 5. Weekly Production (OPTIMIZED: Single Query)
        var sevenDaysAgo = today.AddDays(-6);
        var weeklyRaw = await _context.ProductionOrders
            .AsNoTracking()
            .Where(o => o.CurrentStatus == ProductionStatus.Completed && o.CompletedAt >= sevenDaysAgo)
            .GroupBy(o => o.CompletedAt!.Value.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(x => x.Quantity) })
            .ToListAsync(ct);

        var weeklyData = new List<int>();
        var weeklyLabels = new List<string>();
        for (int i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var totalForDay = weeklyRaw.FirstOrDefault(x => x.Date == date)?.Total ?? 0;
            
            weeklyData.Add(totalForDay);
            weeklyLabels.Add(date.ToString("ddd", ptBr).Replace(".", ""));
        }

        // 6. Profitable Models
        var productStats = await _context.ProductionOrders
            .AsNoTracking()
            .Where(o => o.CurrentStatus == ProductionStatus.Completed)
            .GroupBy(o => o.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                AvgMargin = g.Average(x => x.ProfitMargin)
            })
            .ToListAsync(ct);

        var productIds = productStats.Select(x => x.ProductId).ToList();
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => new { p.Name, p.MainSku }, ct);

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

        // 7. Stalled Stock
        var sixtyDaysAgo = now.AddDays(-60);
        var activeProductIds = await _context.ProductionOrders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= sixtyDaysAgo)
            .Select(o => o.ProductId)
            .Distinct()
            .ToListAsync(ct);

        var stalledProducts = await _context.Products
            .AsNoTracking()
            .Where(p => !activeProductIds.Contains(p.Id))
            .Select(p => new StalledProductDto
            {
                Sku = p.MainSku,
                Name = p.Name,
                DaysSinceLastProduction = 60
            })
            .Take(10)
            .ToListAsync(ct);

        return new DashboardCompleteResponse
        {
            MonthProductionQuantity = monthProduction,
            MonthAverageCostPerPiece = Math.Round(avgCost, 2),
            MonthAverageMargin = Math.Round(avgMargin, 2),
            DelayedOrdersCount = delayedCount,
            ProductionByWorkshop = prodByWorkshop,
            TopProfitableModels = profitabilityList.Take(5).ToList(),
            BottomProfitableModels = profitabilityList.OrderBy(p => p.AverageMargin).Take(5).ToList(),
            StalledStock = stalledProducts,
            WeeklyVolumeData = weeklyData,
            WeeklyLabels = weeklyLabels
        };
    }
}
