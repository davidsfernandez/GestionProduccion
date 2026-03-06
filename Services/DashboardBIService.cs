/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

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

        // 1. Month Production (Sum of all outputs/events this month)
        var monthProduction = await _context.ProductionOrderOutputs
            .AsNoTracking()
            .Where(o => o.CreatedAt >= firstDayOfMonth)
            .SumAsync(o => o.Quantity, ct);

        // 2. Average Cost & Margin (Still based on completed orders as final cost is calculated then)
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

        // 4. Production by Workshop (Based on Outputs)
        var prodByWorkshop = await _context.ProductionOrderOutputs
            .AsNoTracking()
            .Include(o => o.ResponsibleUser)
            .Where(o => o.CreatedAt >= firstDayOfMonth)
            .GroupBy(o => o.ResponsibleUser!.FullName)
            .Select(g => new WorkshopProductionDto
            {
                WorkshopName = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .ToListAsync(ct);

        // 5. Weekly Production (Based on Outputs)
        var sevenDaysAgo = today.AddDays(-6);
        var weeklyRaw = await _context.ProductionOrderOutputs
            .AsNoTracking()
            .Where(o => o.CreatedAt >= sevenDaysAgo)
            .GroupBy(o => o.CreatedAt.Date)
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

        // 6. Team Ranking (Based on Outputs joined with Orders)
        var teamRanking = await _context.ProductionOrderOutputs
            .AsNoTracking()
            .Join(_context.ProductionOrders,
                output => output.ProductionOrderId,
                order => order.Id,
                (output, order) => new { output, order })
            .Where(x => x.order.SewingTeamId != null && x.output.CreatedAt >= firstDayOfMonth)
            .Join(_context.SewingTeams,
                x => x.order.SewingTeamId,
                team => team.Id,
                (x, team) => new { x.output, team })
            .GroupBy(x => x.team.Name)
            .Select(g => new TeamRankingDto
            {
                TeamName = g.Key,
                TotalProduced = g.Sum(x => x.output.Quantity),
                Efficiency = 100 // Efficiency logic can be expanded
            })
            .OrderByDescending(r => r.TotalProduced)
            .ToListAsync(ct);

        // 7. Profitable Models
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
            .OrderBy(p => p.Sku)
            .Take(10)
            .ToListAsync(ct);

        return new DashboardCompleteResponse
        {
            MonthProductionQuantity = monthProduction,
            MonthAverageCostPerPiece = Math.Round(avgCost, 2),
            MonthAverageMargin = Math.Round(avgMargin, 2),
            DelayedOrdersCount = delayedCount,
            ProductionByWorkshop = prodByWorkshop,
            TeamRanking = teamRanking,
            TopProfitableModels = profitabilityList.Take(5).ToList(),
            BottomProfitableModels = profitabilityList.OrderBy(p => p.AverageMargin).Take(5).ToList(),
            StalledStock = stalledProducts,
            WeeklyVolumeData = weeklyData,
            WeeklyLabels = weeklyLabels
        };
    }
}


