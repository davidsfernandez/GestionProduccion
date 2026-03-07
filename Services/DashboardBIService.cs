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

        // 1. Production metrics based on Outputs (Registros de producción parcial)
        var monthProduction = await _context.ProductionOrderOutputs
            .AsNoTracking()
            .Where(o => o.CreatedAt >= firstDayOfMonth)
            .SumAsync(o => o.Quantity, ct);

        // 2. Financial Metrics (Finalized orders only)
        var completedOrders = await _context.ProductionOrders
            .AsNoTracking()
            .Where(o => o.CurrentStatus == ProductionStatus.Completed && o.CompletedAt >= firstDayOfMonth)
            .Select(o => new { o.AverageCostPerPiece, o.ProfitMargin })
            .ToListAsync(ct);

        decimal avgCost = completedOrders.Any() ? completedOrders.Average(o => o.AverageCostPerPiece) : 0;
        decimal avgMargin = completedOrders.Any() ? completedOrders.Average(o => o.ProfitMargin) : 0;

        // 3. Operational status
        var delayedCount = await _context.ProductionOrders
            .AsNoTracking()
            .Where(o => (o.CurrentStatus == ProductionStatus.Pending || o.CurrentStatus == ProductionStatus.InProduction)
                        && o.EstimatedCompletionAt < now)
            .CountAsync(ct);

        // 4. Production by Workshop/Operator (Based on Actual Work registered)
        var prodByWorkshop = await _context.ProductionOrderOutputs
            .AsNoTracking()
            .Include(o => o.ResponsibleUser)
            .Where(o => o.CreatedAt >= firstDayOfMonth)
            .GroupBy(o => o.ResponsibleUser != null ? o.ResponsibleUser.FullName : "Externo")
            .Select(g => new WorkshopProductionDto
            {
                WorkshopName = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.Quantity)
            .ToListAsync(ct);

        // 5. Weekly Volume Time-Series
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
            weeklyLabels.Add(date.ToString("ddd", ptBr).ToUpper().Replace(".", ""));
        }

        // 6. Real Performance Ranking
        // Score logic: 10 points per 100 pieces + bonus for variety? Simple sum for now.
        var ranking = await _context.ProductionOrderOutputs
            .AsNoTracking()
            .Include(o => o.ResponsibleUser)
            .Where(o => o.CreatedAt >= firstDayOfMonth)
            .GroupBy(o => new { o.UserId, o.ResponsibleUser!.FullName })
            .Select(g => new TeamRankingDto
            {
                TeamName = g.Key.FullName,
                TotalProduced = g.Sum(x => x.Quantity),
                Efficiency = Math.Min(100, (int)(g.Sum(x => x.Quantity) / 10.0)) // Placeholder real logic
            })
            .OrderByDescending(r => r.TotalProduced)
            .Take(10)
            .ToListAsync(ct);

        // 7. Product Insights
        var topModels = await _context.ProductionOrders
            .AsNoTracking()
            .Where(o => o.CurrentStatus == ProductionStatus.Completed)
            .GroupBy(o => new { o.ProductId, o.Product!.Name, o.Product!.MainSku })
            .Select(g => new ProductProfitabilityDto
            {
                Sku = g.Key.MainSku,
                Name = g.Key.Name,
                AverageMargin = g.Average(x => x.ProfitMargin)
            })
            .OrderByDescending(x => x.AverageMargin)
            .Take(5)
            .ToListAsync(ct);

        return new DashboardCompleteResponse
        {
            MonthProductionQuantity = monthProduction,
            MonthAverageCostPerPiece = Math.Round(avgCost, 2),
            MonthAverageMargin = Math.Round(avgMargin, 2),
            DelayedOrdersCount = delayedCount,
            ProductionByWorkshop = prodByWorkshop,
            TeamRanking = ranking,
            TopProfitableModels = topModels,
            WeeklyVolumeData = weeklyData,
            WeeklyLabels = weeklyLabels,
            StalledStock = new List<StalledProductDto>() // To be implemented in inventory refactor
        };
    }
}
