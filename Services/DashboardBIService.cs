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

        // 1. General Metrics
        var monthProduction = await _context.ProductionOrderOutputs
            .Where(o => o.CreatedAt >= firstDayOfMonth)
            .SumAsync(o => o.Quantity, ct);

        var completedOrdersQuery = _context.ProductionOrders
            .Where(o => o.CurrentStatus == ProductionStatus.Completed && o.CompletedAt >= firstDayOfMonth);

        var completedOrdersData = await completedOrdersQuery
            .Select(o => new { o.AverageCostPerPiece, o.ProfitMargin })
            .ToListAsync(ct);

        decimal avgCost = completedOrdersData.Any() ? completedOrdersData.Average(o => o.AverageCostPerPiece) : 0;
        decimal avgMargin = completedOrdersData.Any() ? completedOrdersData.Average(o => o.ProfitMargin) : 0;

        var delayedCount = await _context.ProductionOrders
            .AsNoTracking()
            .Where(o => (o.CurrentStatus == ProductionStatus.Pending || o.CurrentStatus == ProductionStatus.InProduction)
                        && o.EstimatedCompletionAt < now)
            .CountAsync(ct);

        // 2. Weekly Production Graph (Hybrid: Outputs + Legacy Orders)
        var sevenDaysAgo = today.AddDays(-6);
        var weeklyOutputs = await _context.ProductionOrderOutputs
            .AsNoTracking()
            .Where(o => o.CreatedAt >= sevenDaysAgo)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(x => x.Quantity) })
            .ToListAsync(ct);

        var orderIdsWithOutputs = await _context.ProductionOrderOutputs
            .AsNoTracking()
            .Where(o => o.CreatedAt >= sevenDaysAgo)
            .Select(o => o.ProductionOrderId)
            .Distinct()
            .ToListAsync(ct);

        var weeklyLegacy = await _context.ProductionOrders
            .AsNoTracking()
            .Where(o => o.CurrentStatus == ProductionStatus.Completed && o.CompletedAt >= sevenDaysAgo)
            .ToListAsync(ct);
        
        var filteredLegacy = weeklyLegacy
            .Where(o => !orderIdsWithOutputs.Contains(o.Id))
            .GroupBy(o => o.CompletedAt!.Value.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(x => x.Quantity) })
            .ToList();

        var weeklyData = new List<int>();
        var weeklyLabels = new List<string>();
        for (int i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var fromOutputs = weeklyOutputs.FirstOrDefault(x => x.Date == date)?.Total ?? 0;
            var fromLegacy = filteredLegacy.FirstOrDefault(x => x.Date == date)?.Total ?? 0;
            
            weeklyData.Add(fromOutputs + fromLegacy);
            weeklyLabels.Add(date.ToString("ddd", ptBr).ToUpper().Replace(".", ""));
        }

        // 3. RANKING INDIVIDUAL (Operators) - Optimized for Hall of Fame
        var operatorRanking = await _context.ProductionOrderOutputs
            .Include(o => o.ResponsibleUser)
            .Where(o => o.CreatedAt >= firstDayOfMonth)
            .GroupBy(o => new { o.UserId, o.ResponsibleUser!.FullName, o.ResponsibleUser.AvatarUrl })
            .Select(g => new RankingEntryDto
            {
                UserName = g.Key.FullName,
                AvatarUrl = g.Key.AvatarUrl ?? "/img/avatars/avatar.jpg",
                CompletedOrders = g.Select(x => x.ProductionOrderId).Distinct().Count(),
                CompletedTasks = g.Sum(x => x.Quantity),
                Score = Math.Min(100, (double)g.Sum(x => x.Quantity) / 10.0) // Scoring normalized
            })
            .OrderByDescending(r => r.CompletedTasks)
            .Take(5)
            .ToListAsync(ct);

        // 4. RANKING POR EQUIPE (Teams) - Optimized
        var teamRanking = await _context.ProductionOrderOutputs
            .Include(o => o.ResponsibleUser)
            .ThenInclude(u => u.SewingTeam)
            .Where(o => o.CreatedAt >= firstDayOfMonth)
            .GroupBy(o => new { 
                Id = o.ResponsibleUser!.SewingTeamId, 
                Name = o.ResponsibleUser.SewingTeam != null ? o.ResponsibleUser.SewingTeam.Name : "Sem Equipe" 
            })
            .Select(g => new TeamRankingDto
            {
                TeamName = g.Key.Name,
                TotalProduced = g.Sum(x => x.Quantity),
                Efficiency = Math.Min(100, (double)g.Sum(x => x.Quantity) / 50.0) // Normalized
            })
            .OrderByDescending(t => t.TotalProduced)
            .Take(5)
            .ToListAsync(ct);

        // 5. PRODUCTION BY WORKSHOP (The "Carga por Operadores" chart)
        var prodByWorkshop = await _context.ProductionOrderOutputs
            .Include(o => o.ResponsibleUser)
            .Where(o => o.CreatedAt >= firstDayOfMonth)
            .GroupBy(o => o.ResponsibleUser!.FullName)
            .Select(g => new WorkshopProductionDto
            {
                WorkshopName = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(10)
            .ToListAsync(ct);

        // 6. Product Insights (Most and Least Profitable)
        var profitabilityData = await _context.ProductionOrders
            .Where(o => o.CurrentStatus == ProductionStatus.Completed)
            .Include(o => o.Product)
            .GroupBy(o => new { o.ProductId, o.Product!.Name, o.Product!.MainSku })
            .Select(g => new ProductProfitabilityDto
            {
                Sku = g.Key.MainSku,
                Name = g.Key.Name,
                AverageMargin = g.Average(x => x.ProfitMargin)
            })
            .ToListAsync(ct);

        var topModels = profitabilityData.OrderByDescending(x => x.AverageMargin).Take(5).ToList();
        
        // Avoid showing same items in bottom if they are already in top (common in small datasets)
        var topSkus = topModels.Select(t => t.Sku).ToList();
        var bottomModels = profitabilityData
            .Where(x => !topSkus.Contains(x.Sku))
            .OrderBy(x => x.AverageMargin)
            .Take(5)
            .ToList();

        // 7. Stalled Stock (Products with no orders in last 60 days)
        var sixtyDaysAgo = now.AddDays(-60);
        
        // 1. Get IDs of products seen in recent orders
        var recentIds = await _context.ProductionOrders
            .Where(o => o.CreatedAt >= sixtyDaysAgo)
            .Select(o => o.ProductId)
            .Distinct()
            .ToListAsync(ct);

        // 2. Get all products and filter out the recent ones in memory
        var allProductsList = await _context.Products.AsNoTracking().ToListAsync(ct);
        var stalledList = allProductsList.Where(p => !recentIds.Contains(p.Id)).ToList();

        var stalledStock = new List<StalledProductDto>();
        foreach (var p in stalledList)
        {
            var lastOrder = await _context.ProductionOrders
                .Where(o => o.ProductId == p.Id && o.CurrentStatus == ProductionStatus.Completed)
                .OrderByDescending(o => o.CompletedAt)
                .FirstOrDefaultAsync(ct);

            stalledStock.Add(new StalledProductDto
            {
                Sku = p.MainSku,
                Name = p.Name,
                DaysSinceLastProduction = lastOrder?.CompletedAt != null 
                    ? (int)(now - lastOrder.CompletedAt.Value).TotalDays 
                    : 999
            });
        }


        return new DashboardCompleteResponse
        {
            MonthProductionQuantity = monthProduction,
            MonthAverageCostPerPiece = Math.Round(avgCost, 2),
            MonthAverageMargin = Math.Round(avgMargin, 2),
            DelayedOrdersCount = delayedCount,
            ProductionByWorkshop = prodByWorkshop,
            OperatorRanking = operatorRanking,
            TeamRanking = teamRanking,
            TopProfitableModels = topModels,
            BottomProfitableModels = bottomModels,
            WeeklyVolumeData = weeklyData,
            WeeklyLabels = weeklyLabels,
            StalledStock = stalledStock
        };
    }
}
