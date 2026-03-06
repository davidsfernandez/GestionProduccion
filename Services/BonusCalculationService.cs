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
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GestionProduccion.Services;

public class BonusCalculationService : IBonusCalculationService
{
    private readonly ISewingTeamRepository _teamRepo;
    private readonly IProductionOrderRepository _orderRepo;
    private readonly IBonusRuleRepository _ruleRepo;
    private readonly IQAService _qaService;
    private readonly IProductionOrderOutputRepository _outputRepo;

    public BonusCalculationService(
        ISewingTeamRepository teamRepo,
        IProductionOrderRepository orderRepo,
        IBonusRuleRepository ruleRepo,
        IQAService qaService,
        IProductionOrderOutputRepository outputRepo)
    {
        _teamRepo = teamRepo;
        _orderRepo = orderRepo;
        _ruleRepo = ruleRepo;
        _qaService = qaService;
        _outputRepo = outputRepo;
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
        
        if (!outputsList.Any())
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

        // 2. Sum productivity from events
        int totalProduced = outputsList.Sum(o => o.Quantity);
        
        // 3. For performance (On-Time), we still look at the parent orders involved in these outputs
        var involvedOrderIds = outputs.Select(o => o.ProductionOrderId).Distinct();
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

        // --- CALCULATIONS ---

        // 1. Productivity (Based on meta if available, else standard bonus if they produced anything)
        decimal productivityBonus = (decimal)rule.ProductivityPercentage;

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

    public async Task<BonusReportDto> CalculateUserBonusAsync(int userId, DateTime startDate, DateTime endDate)
    {
        var user = await _teamRepo.GetMemberByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        var rule = await _ruleRepo.GetActiveRuleAsync() ?? new BonusRule();

        // 1. Get all partial/total outputs for this user in the date range
        var outputs = await _outputRepo.GetByUserAndDateRangeAsync(userId, startDate, endDate);

        if (!outputs.Any())
        {
            return new BonusReportDto
            {
                TeamName = user.FullName,
                FinalBonusPercentage = 0,
                TotalProduced = 0,
                CompletedOrders = 0
            };
        }

        int totalProduced = outputs.Sum(o => o.Quantity);
        var involvedOrderIds = outputs.Select(o => o.ProductionOrderId).Distinct();
        
        int totalDefects = 0;
        foreach (var orderId in involvedOrderIds)
        {
            var defects = await _qaService.GetDefectsByOrderAsync(orderId);
            totalDefects += defects
                .Where(d => d.ReportedAt >= startDate && d.ReportedAt <= endDate && d.ReportedByUserId == userId)
                .Sum(d => d.Quantity);
        }

        decimal productivityBonus = totalProduced > 0 ? (decimal)rule.ProductivityPercentage : 0;
        decimal defectRatio = totalProduced > 0 ? (decimal)totalDefects / totalProduced * 100 : 0;

        decimal individualBonus = productivityBonus;
        if (defectRatio > 5) individualBonus = 0;

        // 2. Get Team Bonus Share (if part of a team)
        decimal teamShare = 0;
        if (user.SewingTeamId.HasValue)
        {
            var teamReport = await CalculateTeamBonusAsync(user.SewingTeamId.Value, startDate, endDate);
            var team = await _teamRepo.GetTeamWithMembersAsync(user.SewingTeamId.Value);
            int teamMembersCount = team?.Members.Count ?? 1;
            teamShare = teamReport.FinalBonusPercentage / Math.Max(1, teamMembersCount);
        }

        decimal finalBonus = individualBonus + teamShare;

        return new BonusReportDto
        {
            TeamName = user.FullName,
            ProductivityPercentage = individualBonus,
            DefectPercentage = Math.Round(defectRatio, 2),
            FinalBonusPercentage = Math.Round(finalBonus, 2),
            CompletedOrders = involvedOrderIds.Count(),
            TotalProduced = totalProduced,
            TotalDefects = totalDefects
        };
    }
}


