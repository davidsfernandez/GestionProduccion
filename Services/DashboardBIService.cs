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
    private const double OperatorMonthlyTarget = 1000.0;
    private const double TeamMonthlyTarget = 5000.0;

    public DashboardBIService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardCompleteResponse> GetCompleteDashboardAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var startOfToday = now.Date;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var ptBr = new System.Globalization.CultureInfo("pt-BR");

        // 1. General Metrics - Hybrid Aggregation: Sum(Qty) per (Order, Size, Stage), then Max across Stages, then Sum.
        var monthProduction = await _context.ProductionOrderOutputs
            .Where(o => o.CreatedAt >= firstDayOfMonth)
            .GroupBy(o => new { o.ProductionOrderId, o.ProductionOrderSizeId, o.Stage })
            .Select(g => new { g.Key.ProductionOrderId, g.Key.ProductionOrderSizeId, StageTotal = g.Sum(x => x.Quantity) })
            .GroupBy(x => new { x.ProductionOrderId, x.ProductionOrderSizeId })
            .Select(g => g.Max(x => x.StageTotal))
            .SumAsync(ct);

        var completedOrdersQuery = _context.ProductionOrders
            .Where(o => (o.CurrentStatus == ProductionStatus.Completed || o.CurrentStatus == ProductionStatus.Finished) 
                        && o.CompletedAt >= firstDayOfMonth);

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
        var sevenDaysAgo = startOfToday.AddDays(-6);
        var weeklyOutputsRaw = await _context.ProductionOrderOutputs
            .AsNoTracking()
            .Where(o => o.CreatedAt >= sevenDaysAgo)
            .GroupBy(o => new { Date = o.CreatedAt.Date, o.ProductionOrderId, o.ProductionOrderSizeId, o.Stage })
            .Select(g => new { g.Key.Date, g.Key.ProductionOrderId, g.Key.ProductionOrderSizeId, StageTotal = g.Sum(x => x.Quantity) })
            .GroupBy(x => new { x.Date, x.ProductionOrderId, x.ProductionOrderSizeId })
            .Select(g => new { g.Key.Date, MaxQty = g.Max(x => x.StageTotal) })
            .ToListAsync(ct);

        var weeklyOutputs = weeklyOutputsRaw
            .GroupBy(x => x.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(x => x.MaxQty) })
            .ToList();

        var orderIdsWithOutputs = await _context.ProductionOrderOutputs
            .AsNoTracking()
            .Where(o => o.CreatedAt >= sevenDaysAgo)
            .Select(o => o.ProductionOrderId)
            .Distinct()
            .ToListAsync(ct);

        var weeklyLegacy = await _context.ProductionOrders
            .AsNoTracking()
            .Where(o => (o.CurrentStatus == ProductionStatus.Completed || o.CurrentStatus == ProductionStatus.Finished) 
                        && o.CompletedAt >= sevenDaysAgo)
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
            var date = startOfToday.AddDays(-i);
            var fromOutputs = weeklyOutputs.FirstOrDefault(x => x.Date == date)?.Total ?? 0;
            var fromLegacy = filteredLegacy.FirstOrDefault(x => x.Date == date)?.Total ?? 0;
            
            weeklyData.Add(fromOutputs + fromLegacy);
            weeklyLabels.Add(date.ToString("ddd", ptBr).ToUpper().Replace(".", ""));
        }

        // 3. RANKING INDIVIDUAL (Operators) - Optimized for Hall of Fame
        var operatorRankingData = await _context.ProductionOrderOutputs
            .Where(o => o.CreatedAt >= firstDayOfMonth)
            .GroupBy(o => new { 
                o.UserId, 
                o.ProductionOrderId, 
                o.ProductionOrderSizeId,
                o.Stage,
                o.ResponsibleUser!.FullName, 
                o.ResponsibleUser.AvatarUrl,
                o.ProductionOrder!.CurrentStatus 
            })
            .Select(g => new {
                g.Key.UserId,
                g.Key.FullName,
                g.Key.AvatarUrl,
                g.Key.ProductionOrderId,
                g.Key.ProductionOrderSizeId,
                g.Key.CurrentStatus,
                StageTotal = g.Sum(x => x.Quantity)
            })
            .GroupBy(x => new { x.UserId, x.FullName, x.AvatarUrl, x.ProductionOrderId, x.ProductionOrderSizeId, x.CurrentStatus })
            .Select(g => new {
                g.Key.UserId,
                g.Key.FullName,
                g.Key.AvatarUrl,
                g.Key.ProductionOrderId,
                g.Key.CurrentStatus,
                MaxQty = g.Max(x => x.StageTotal)
            })
            .ToListAsync(ct);

        var operatorRanking = operatorRankingData
            .GroupBy(o => new { o.UserId, o.FullName, o.AvatarUrl })
            .Select(g => {
                var completedOrders = g.Where(x => x.CurrentStatus == ProductionStatus.Completed || x.CurrentStatus == ProductionStatus.Finished).ToList();
                var totalProduced = completedOrders.Sum(x => x.MaxQty);

                return new RankingEntryDto
                {
                    UserName = g.Key.FullName,
                    AvatarUrl = g.Key.AvatarUrl ?? "/img/avatars/avatar.jpg",
                    CompletedOrders = completedOrders.Select(x => x.ProductionOrderId).Distinct().Count(),
                    CompletedTasks = totalProduced,
                    Score = Math.Min(100, (double)totalProduced / (OperatorMonthlyTarget / 100.0))
                };
            })
            .OrderByDescending(r => r.CompletedTasks)
            .Take(5)
            .ToList();

        // 4. RANKING POR EQUIPE (Teams) - Optimized
        var teamRankingData = await _context.ProductionOrderOutputs
            .Where(o => o.CreatedAt >= firstDayOfMonth)
            .GroupBy(o => new { 
                Id = o.ResponsibleUser!.SewingTeamId, 
                Name = o.ResponsibleUser.SewingTeam != null ? o.ResponsibleUser.SewingTeam.Name : "Sem Equipe",
                o.ProductionOrderId,
                o.ProductionOrderSizeId,
                o.Stage,
                o.ProductionOrder!.CurrentStatus
            })
            .Select(g => new {
                g.Key.Id,
                g.Key.Name,
                g.Key.ProductionOrderId,
                g.Key.ProductionOrderSizeId,
                g.Key.CurrentStatus,
                StageTotal = g.Sum(x => x.Quantity)
            })
            .GroupBy(x => new { x.Id, x.Name, x.ProductionOrderId, x.ProductionOrderSizeId, x.CurrentStatus })
            .Select(g => new {
                g.Key.Id,
                g.Key.Name,
                g.Key.ProductionOrderId,
                g.Key.CurrentStatus,
                MaxQty = g.Max(x => x.StageTotal)
            })
            .ToListAsync(ct);

        var teamRanking = teamRankingData
            .GroupBy(o => new { o.Id, o.Name })
            .Select(g => {
                var completedOrders = g.Where(x => x.CurrentStatus == ProductionStatus.Completed || x.CurrentStatus == ProductionStatus.Finished).ToList();
                var totalProduced = completedOrders.Sum(x => x.MaxQty);

                return new TeamRankingDto
                {
                    TeamName = g.Key.Name,
                    TotalProduced = totalProduced,
                    Efficiency = Math.Min(100, (double)totalProduced / (TeamMonthlyTarget / 100.0))
                };
            })
            .OrderByDescending(t => t.TotalProduced)
            .Take(5)
            .ToList();

        // 5. PRODUCTION BY WORKSHOP (The "Carga por Operadores" chart)
        var workshopRankingData = await _context.ProductionOrderOutputs
            .Where(o => o.CreatedAt >= firstDayOfMonth)
            .GroupBy(o => new { o.ResponsibleUser!.FullName, o.ProductionOrderId, o.ProductionOrderSizeId, o.Stage })
            .Select(g => new { g.Key.FullName, g.Key.ProductionOrderId, g.Key.ProductionOrderSizeId, StageTotal = g.Sum(x => x.Quantity) })
            .GroupBy(x => new { x.FullName, x.ProductionOrderId, x.ProductionOrderSizeId })
            .Select(g => new { g.Key.FullName, MaxQty = g.Max(x => x.StageTotal) })
            .ToListAsync(ct);

        var prodByWorkshop = workshopRankingData
            .GroupBy(o => o.FullName)
            .Select(g => new WorkshopProductionDto
            {
                WorkshopName = g.Key,
                Quantity = g.Sum(x => x.MaxQty)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(10)
            .ToList();

        // 6. Product Insights (Most and Least Profitable)
        var profitabilityData = await _context.ProductionOrders
            .Where(o => o.CurrentStatus == ProductionStatus.Completed || o.CurrentStatus == ProductionStatus.Finished)
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
                .Where(o => o.ProductId == p.Id && (o.CurrentStatus == ProductionStatus.Completed || o.CurrentStatus == ProductionStatus.Finished))
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
