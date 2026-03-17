/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Application.Mappers;
using Microsoft.EntityFrameworkCore;

namespace GestionProduccion.Services;

public class BonusCalculationService : IBonusCalculationService
{
    private readonly ISewingTeamRepository _teamRepo;
    private readonly IProductionOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;
    private readonly IBonusRuleRepository _ruleRepo;
    private readonly IQAService _qaService;
    private readonly IProductionOrderOutputRepository _outputRepo;
    private readonly MainMapper _mapper;

    public BonusCalculationService(
        ISewingTeamRepository teamRepo,
        IProductionOrderRepository orderRepo,
        IProductRepository productRepo,
        IBonusRuleRepository ruleRepo,
        IQAService qaService,
        IProductionOrderOutputRepository outputRepo,
        MainMapper mapper)
    {
        _teamRepo = teamRepo;
        _orderRepo = orderRepo;
        _productRepo = productRepo;
        _ruleRepo = ruleRepo;
        _qaService = qaService;
        _outputRepo = outputRepo;
        _mapper = mapper;
    }

    public async Task<BonusReportDto> CalculateTeamBonusAsync(int teamId, DateTime startDate, DateTime endDate)
    {
        var team = await _teamRepo.GetTeamWithMembersAsync(teamId);
        if (team == null) throw new KeyNotFoundException("Team not found.");

        var rule = await _ruleRepo.GetActiveRuleAsync();
        
        if (rule == null)
        {
            return new BonusReportDto
            {
                TeamId = teamId,
                TeamName = team.Name,
                FinalBonusPercentage = 0,
                TotalAmount = 0,
                Message = "Nenhuma regra de bÃ´nus ativa configurada."
            };
        }

        // 1. Get all partial/total outputs for this team in the date range
        var outputs = await _outputRepo.GetByTeamAndDateRangeAsync(teamId, startDate, endDate);
        var outputsList = outputs.ToList();
        
        // 2. Identify orders completed in this period that DON'T have outputs (legacy/tests)
        var orderIdsWithOutputs = outputsList.Select(o => o.ProductionOrderId).Distinct().ToList();
        var allTeamOrdersInRange = await _orderRepo.GetQueryableAsync();
        var legacyOrders = await allTeamOrdersInRange
            .Where(o => o.SewingTeamId == teamId && o.CompletedAt >= startDate && o.CompletedAt <= endDate && o.CurrentStatus == ProductionStatus.Completed)
            .ToListAsync();
        
        var filteredLegacy = legacyOrders.Where(o => !orderIdsWithOutputs.Contains(o.Id)).ToList();

        if (!outputsList.Any() && !filteredLegacy.Any())
        {
            return new BonusReportDto
            {
                TeamId = teamId,
                TeamName = team.Name,
                FinalBonusPercentage = 0,
                TotalAmount = 0,
                CompletedOrders = 0,
                TotalProduced = 0
            };
        }

        // 3. Combine sources for productivity (AVOID DOUBLE COUNTING)
        var involvedOrderIds = orderIdsWithOutputs.Union(filteredLegacy.Select(o => o.Id)).Distinct().ToList();
        var teamOrders = new List<ProductionOrder>();
        foreach (var id in involvedOrderIds)
        {
            var order = await _orderRepo.GetByIdAsync(id);
            if (order != null) teamOrders.Add(order);
        }

        // For V2, we calculate the monetary amount per order
        decimal totalAmount = 0;
        int totalProduced = 0;

        foreach (var orderId in involvedOrderIds)
        {
            var order = teamOrders.FirstOrDefault(o => o.Id == orderId);
            if (order == null) continue;

            // Get produced quantity for this order in the period
            int orderQtyInPeriod = 0;
            if (orderIdsWithOutputs.Contains(orderId))
            {
                orderQtyInPeriod = outputsList
                    .Where(o => o.ProductionOrderId == orderId)
                    .GroupBy(o => o.ProductionOrderSizeId)
                    .Select(g => g.Max(x => x.Quantity))
                    .Sum();
            }
            else
            {
                orderQtyInPeriod = order.Quantity; // Legacy fallback
            }

            totalProduced += orderQtyInPeriod;

            // Resolve rate: Order Snapshot > Product Default > Rule Base > 1.50
            decimal rate = order.AppliedBonusPerPiece;
            if (rate <= 0)
            {
                var product = await _productRepo.GetByIdAsync(order.ProductId);
                rate = product?.DefaultBonusPerPiece ?? rule.BonusAmount;
                if (rate <= 0) rate = 1.50m;
            }

            totalAmount += orderQtyInPeriod * rate;
        }
        
        // --- ATOMIC BONUS VERIFIER ---
        if (rule.IsAtomicMode)
        {
            foreach (var order in teamOrders)
            {
                var orderOutputs = outputsList.Where(o => o.ProductionOrderId == order.Id).ToList();
                var (isValid, reason) = IsAtomicBonusValid(rule, order, orderOutputs);
                if (!isValid)
                {
                    return new BonusReportDto
                    {
                        TeamId = teamId,
                        TeamName = team.Name,
                        FinalBonusPercentage = 0,
                        TotalAmount = 0,
                        IsAtomicFailure = true,
                        AtomicFailureReason = reason,
                        Message = $"Atomic Mode: Order {order.LotCode} failed: {reason}"
                    };
                }
            }
        }

        int onTimeOrders = teamOrders.Count(o => o.CompletedAt != null && o.CompletedAt <= o.EstimatedCompletionAt);

        // 4. Sum defects from QA Service for the involved orders in this period
        int totalDefects = 0;
        var defectsPerOrder = new Dictionary<int, int>();

        foreach (var orderId in involvedOrderIds)
        {
            var defects = await _qaService.GetDefectsByOrderAsync(orderId);
            // We only count defects reported in this period
            var orderDefects = defects
                .Where(d => d.ReportedAt >= startDate && d.ReportedAt <= endDate)
                .Sum(d => d.Quantity);

            totalDefects += orderDefects;
            defectsPerOrder[orderId] = orderDefects;
        }

        // 1. Efficiency Calculation: (Standard Time / Effective Time)
        double totalStandardMinutes = 0;
        double totalEffectiveMinutes = 0;

        foreach (var order in teamOrders)
        {
            var product = await _productRepo.GetByIdAsync(order.ProductId);
            if (product != null)
            {
                totalStandardMinutes += product.AverageProductionTimeMinutes * order.Quantity;
                totalEffectiveMinutes += order.EffectiveMinutes;
            }
        }

        decimal efficiencyFactor = 1;
        if (totalEffectiveMinutes > 0 && totalStandardMinutes > 0)
        {
            efficiencyFactor = (decimal)(totalStandardMinutes / totalEffectiveMinutes);
            if (efficiencyFactor > 1.5m) efficiencyFactor = 1.5m;
            if (efficiencyFactor < 0.5m) efficiencyFactor = 0.5m;
        }

        // 2. Deadline Performance
        decimal onTimeRatio = teamOrders.Any() ? (decimal)onTimeOrders / teamOrders.Count : 1;
        
        // 3. Quality Factor
        decimal defectRatio = totalProduced > 0 ? (decimal)totalDefects / totalProduced * 100 : 0;
        decimal qualityFactor = 1;

        if (defectRatio > rule.DefectLimitPercentage)
        {
            qualityFactor = 0;
        }
        else if (rule.DefectLimitPercentage > 0)
        {
            qualityFactor = Math.Max(0, 1 - (defectRatio / rule.DefectLimitPercentage));
        }

        // Final Calculation (Scalable V2): Amount * Efficiency * Deadline * Quality
        decimal finalBonusAmount = totalAmount * efficiencyFactor * onTimeRatio * qualityFactor;

        return new BonusReportDto
        {
            TeamId = teamId,
            TeamName = team.Name,
            ProductivityPercentage = Math.Round(efficiencyFactor * 100, 2),
            DeadlinePerformance = Math.Round(onTimeRatio * 100, 2),
            DefectPercentage = Math.Round(defectRatio, 2),
            FinalBonusPercentage = Math.Round(qualityFactor * 100, 2),
            QualityFactor = qualityFactor,
            TotalAmount = Math.Round(finalBonusAmount, 2), 
            CompletedOrders = involvedOrderIds.Count(),
            OnTimeOrders = onTimeOrders,
            TotalProduced = totalProduced,
            TotalDefects = totalDefects,
            Orders = teamOrders.Select(o => new OrderBonusDetail
            {
                LotCode = o.LotCode,
                IsOnTime = o.CompletedAt != null && o.CompletedAt <= o.EstimatedCompletionAt,
                Defects = defectsPerOrder.ContainsKey(o.Id) ? defectsPerOrder[o.Id] : 0, 
                Contribution = 0 // Deprecated in V2
            }).ToList()
        };
    }

    public async Task<BonusReportDto> CalculateUserBonusAsync(int userId, DateTime startDate, DateTime endDate)
    {
        var user = await _teamRepo.GetMemberByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        var rule = await _ruleRepo.GetActiveRuleAsync() ?? new BonusRule();

        // 1. Get all pieces produced by this user in the date range
        var outputs = await _outputRepo.GetByUserAndDateRangeAsync(userId, startDate, endDate);
        var outputsList = outputs.ToList();
        
        var involvedOrderIds = outputsList.Select(o => o.ProductionOrderId).Distinct().ToList();
        var userOrders = new List<ProductionOrder>();
        foreach (var id in involvedOrderIds)
        {
            var order = await _orderRepo.GetByIdAsync(id);
            if (order != null) userOrders.Add(order);
        }

        // V2: Calculate monetary amount based on individual contribution
        decimal totalAmount = 0;
        int totalProduced = 0;

        foreach (var orderId in involvedOrderIds)
        {
            var order = userOrders.FirstOrDefault(o => o.Id == orderId);
            if (order == null) continue;

            int userQtyForOrder = outputsList
                .Where(o => o.ProductionOrderId == orderId)
                .GroupBy(o => o.ProductionOrderSizeId)
                .Select(g => g.Max(x => x.Quantity))
                .Sum();

            totalProduced += userQtyForOrder;

            decimal rate = order.AppliedBonusPerPiece;
            if (rate <= 0)
            {
                var product = await _productRepo.GetByIdAsync(order.ProductId);
                rate = product?.DefaultBonusPerPiece ?? rule.BonusAmount;
                if (rate <= 0) rate = 1.50m;
            }

            totalAmount += userQtyForOrder * rate;
        }

        // --- ATOMIC BONUS VERIFIER ---
        if (rule.IsAtomicMode)
        {
            foreach (var order in userOrders)
            {
                var orderOutputs = outputsList.Where(o => o.ProductionOrderId == order.Id).ToList();
                var (isValid, reason) = IsAtomicBonusValid(rule, order, orderOutputs);
                if (!isValid)
                {
                    return new BonusReportDto
                    {
                        TeamName = user.FullName,
                        FinalBonusPercentage = 0,
                        TotalAmount = 0,
                        IsAtomicFailure = true,
                        AtomicFailureReason = reason,
                        Message = $"Atomic Mode: Order {order.LotCode} failed: {reason}"
                    };
                }
            }
        }

        // 2. Individual Base Metrics (Quality attribution)
        int totalDefects = 0;
        var userDefectsPerOrder = new Dictionary<int, int>();

        foreach (var orderId in involvedOrderIds)
        {
            var defects = await _qaService.GetDefectsByOrderAsync(orderId);
            var userOrderDefects = defects
                .Where(d => d.ReportedAt >= startDate && d.ReportedAt <= endDate && d.ResponsibleUserId == userId)
                .Sum(d => d.Quantity);

            totalDefects += userOrderDefects;
            userDefectsPerOrder[orderId] = userOrderDefects;
        }

        // Quality Denominator: Sum of all operations performed (real effort)
        int totalOperationsInPeriod = outputsList.Sum(o => o.Quantity);
        decimal indDefectRatio = totalOperationsInPeriod > 0 ? (decimal)totalDefects / totalOperationsInPeriod * 100 : 0;
        
        // Quality Multiplier (Progressive)
        decimal indQualityFactor = 1;
        if (rule.DefectLimitPercentage > 0)
        {
            indQualityFactor = Math.Max(0, 1 - (indDefectRatio / rule.DefectLimitPercentage));
        }
        else if (indDefectRatio > 0)
        {
            indQualityFactor = 0;
        }

        decimal finalBonusAmount = totalAmount * indQualityFactor;

        return new BonusReportDto
        {
            TeamName = user.FullName,
            ProductivityPercentage = 100, // Fixed base for individual in V2
            DeadlinePerformance = 100,
            DefectPercentage = Math.Round(indDefectRatio, 2),
            FinalBonusPercentage = Math.Round(indQualityFactor * 100, 2),
            TotalAmount = Math.Round(finalBonusAmount, 2),
            CompletedOrders = involvedOrderIds.Count(),
            TotalProduced = totalProduced,
            TotalDefects = totalDefects,
            QualityFactor = indQualityFactor,
            Orders = userOrders.Select(o => new OrderBonusDetail
            {
                LotCode = o.LotCode,
                IsOnTime = o.CompletedAt != null && o.CompletedAt <= o.EstimatedCompletionAt,
                Defects = userDefectsPerOrder.ContainsKey(o.Id) ? userDefectsPerOrder[o.Id] : 0,
                Contribution = 0
            }).ToList()
        };
    }

    private (bool isValid, string? reason) IsAtomicBonusValid(BonusRule rule, ProductionOrder order, List<ProductionOrderOutput> orderOutputs)
    {
        if (!rule.IsAtomicMode) return (true, null);

        // Legacy check: orders created before the rule was last updated are exempt
        if (order.CreatedAt < rule.UpdatedAt) return (true, null);

        int sumProduced = orderOutputs.Sum(o => o.Quantity);
        if (sumProduced < order.Quantity) return (false, "MISSING_PIECES");

        if (!orderOutputs.Any()) return (false, "MISSING_PIECES");

        var lastPieceTimestamp = orderOutputs.Max(o => o.CreatedAt);
        var deadline = order.EstimatedCompletionAt.Date.AddDays(1);

        if (lastPieceTimestamp >= deadline) return (false, "LATE_DELIVERY");

        return (true, null);
    }
}


