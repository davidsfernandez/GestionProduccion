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
        // We sum the maximum pieces produced in any stage for each order to get real production volume
        int totalProducedFromOutputs = outputsList
            .GroupBy(o => o.ProductionOrderId)
            .Select(g => g.Max(x => x.Quantity))
            .Sum();

        int totalProduced = totalProducedFromOutputs + filteredLegacy.Sum(o => o.Quantity);
        
        // 4. Identify all involved orders (from outputs or legacy)
        var involvedOrderIds = orderIdsWithOutputs.Union(filteredLegacy.Select(o => o.Id)).Distinct().ToList();
        var teamOrders = new List<ProductionOrder>();
        foreach(var id in involvedOrderIds)
        {
            var order = await _orderRepo.GetByIdAsync(id);
            if (order != null) teamOrders.Add(order);
        }

        int onTimeOrders = teamOrders.Count(o => o.CompletedAt != null && o.CompletedAt <= o.EstimatedCompletionAt);

        // 4. Sum defects from QA Service for the involved orders in this period
        int totalDefects = 0;
        foreach (var orderId in involvedOrderIds)
        {
            var defects = await _qaService.GetDefectsByOrderAsync(orderId);
            // We only count defects reported in this period
            totalDefects += defects
                .Where(d => d.ReportedAt >= startDate && d.ReportedAt <= endDate)
                .Sum(d => d.Quantity);
        }

        // 1. Productivity Calculation: (Standard Time / Effective Time) * Rule Percentage
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
            // Cap efficiency factor between 0.5 and 1.5 to avoid extreme bonus fluctuations
            if (efficiencyFactor > 1.5m) efficiencyFactor = 1.5m;
            if (efficiencyFactor < 0.5m) efficiencyFactor = 0.5m;
        }

        decimal productivityBonus = (decimal)rule.ProductivityPercentage * efficiencyFactor;

        // 2. Deadline Performance
        decimal onTimeRatio = teamOrders.Any() ? (decimal)onTimeOrders / teamOrders.Count : 1;
        decimal deadlineBonus = onTimeRatio * rule.DeadlineBonusPercentage;

        // 3. Quality Penalty
        decimal defectRatio = totalProduced > 0 ? (decimal)totalDefects / totalProduced * 100 : 0;
        decimal finalBonus = productivityBonus + deadlineBonus;

        if (defectRatio > rule.DefectLimitPercentage)
        {
            finalBonus = 0;
        }

        if (finalBonus < 0) finalBonus = 0;

        return new BonusReportDto
        {
            TeamId = teamId,
            TeamName = team.Name,
            ProductivityPercentage = productivityBonus,
            DeadlinePerformance = Math.Round(onTimeRatio * 100, 2),
            DefectPercentage = Math.Round(defectRatio, 2),
            FinalBonusPercentage = Math.Round(finalBonus, 2),
            TotalAmount = 0, 
            CompletedOrders = involvedOrderIds.Count(), // Number of orders they worked on
            OnTimeOrders = onTimeOrders,
            TotalProduced = totalProduced,
            TotalDefects = totalDefects,
            Orders = teamOrders.Select(o => new OrderBonusDetail
            {
                LotCode = o.LotCode,
                IsOnTime = o.CompletedAt != null && o.CompletedAt <= o.EstimatedCompletionAt,
                Defects = 0, 
                Contribution = Math.Round(finalBonus / Math.Max(1, involvedOrderIds.Count()), 2)
            }).ToList()
        };
    }

    public async Task<BonusReportDto> CalculateUserBonusAsync(int userId, DateTime startDate, DateTime endDate, bool isProfessional = false)
    {
        var user = await _teamRepo.GetMemberByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        var rule = await _ruleRepo.GetActiveRuleAsync() ?? new BonusRule();

        // 1. Get all partial/total outputs for this user in the date range
        var outputs = await _outputRepo.GetByUserAndDateRangeAsync(userId, startDate, endDate);
        var outputsList = outputs.ToList();
        
        // AVOID DOUBLE COUNTING: Sum max pieces in any stage per order
        int totalProduced = outputsList
            .GroupBy(o => o.ProductionOrderId)
            .Select(g => g.Max(x => x.Quantity))
            .Sum();

        var involvedOrderIds = outputsList.Select(o => o.ProductionOrderId).Distinct().ToList();

        // 2. Individual Base Metrics (Destrez Técnica)
        int totalDefects = 0;
        foreach (var orderId in involvedOrderIds)
        {
            var defects = await _qaService.GetDefectsByOrderAsync(orderId);
            totalDefects += defects
                .Where(d => d.ReportedAt >= startDate && d.ReportedAt <= endDate && d.ResponsibleUserId == userId)
                .Sum(d => d.Quantity);
        }

        // Productivity Factor (Ind)
        decimal indProductivityBonus = totalProduced > 0 ? (decimal)rule.ProductivityPercentage : 0;
        
        // Quality Denominator: Sum of all pieces handled in all stages (real effort)
        int totalOperationsInPeriod = outputsList.Sum(o => o.Quantity);
        decimal indDefectRatio = totalOperationsInPeriod > 0 ? (decimal)totalDefects / totalOperationsInPeriod * 100 : 0;
        
        // Quality Multiplier (Progressive): 
        // If ratio is 0, multiplier is 1. If ratio >= limit, multiplier is 0.
        decimal indQualityFactor = 1;
        if (rule.DefectLimitPercentage > 0)
        {
            indQualityFactor = Math.Max(0, 1 - (indDefectRatio / rule.DefectLimitPercentage));
        }
        else if (indDefectRatio > 0)
        {
            indQualityFactor = 0;
        }

        decimal individualPurityResult = indProductivityBonus * indQualityFactor;

        decimal finalBonus = 0;
        decimal teamContribution = 0;
        decimal individualContribution = 0;
        decimal deadlinePerformance = totalProduced > 0 ? 100 : 0;

        // 3. Handle Modes
        if (!isProfessional)
        {
            // PURE INDIVIDUAL MODE: 100% individual effort
            finalBonus = individualPurityResult;
            individualContribution = finalBonus;
        }
        else if (user.SewingTeamId.HasValue)
        {
            // PROFESSIONAL MODE (HYBRID): 70% Individual / 30% Team
            var teamReport = await CalculateTeamBonusAsync(user.SewingTeamId.Value, startDate, endDate);
            
            individualContribution = individualPurityResult * 0.7m;
            teamContribution = teamReport.FinalBonusPercentage * 0.3m;
            
            finalBonus = individualContribution + teamContribution;
            
            // Sync radar metrics for UI
            deadlinePerformance = teamReport.DeadlinePerformance; 
            
            // If individual has no data, they inherit team performance but weighted down
            if (totalProduced == 0)
            {
                indDefectRatio = teamReport.DefectPercentage;
                totalProduced = teamReport.TotalProduced / 10; // Nominal display
            }
        }

        return new BonusReportDto
        {
            TeamName = user.FullName,
            ProductivityPercentage = Math.Round(indProductivityBonus, 2),
            DeadlinePerformance = Math.Round(deadlinePerformance, 2),
            DefectPercentage = Math.Round(indDefectRatio, 2),
            FinalBonusPercentage = Math.Round(finalBonus, 2),
            IndividualContribution = Math.Round(individualContribution, 2),
            TeamContribution = Math.Round(teamContribution, 2),
            CompletedOrders = involvedOrderIds.Count(),
            TotalProduced = totalProduced,
            TotalDefects = totalDefects,
            QualityFactor = indQualityFactor
        };
    }
}


