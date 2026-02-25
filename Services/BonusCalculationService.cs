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

    public BonusCalculationService(
        ISewingTeamRepository teamRepo,
        IProductionOrderRepository orderRepo,
        IBonusRuleRepository ruleRepo,
        IQAService qaService)
    {
        _teamRepo = teamRepo;
        _orderRepo = orderRepo;
        _ruleRepo = ruleRepo;
        _qaService = qaService;
    }

    public async Task<BonusReportDto> CalculateTeamBonusAsync(int teamId, DateTime startDate, DateTime endDate)
    {
        var team = await _teamRepo.GetTeamWithMembersAsync(teamId);
        if (team == null) throw new KeyNotFoundException("Team not found.");

        var rule = await _ruleRepo.GetActiveRuleAsync() ?? new BonusRule();

        // Query completed orders within range for this team (Server-Side Evaluation)
        var query = await _orderRepo.GetQueryableAsync();
        var teamOrders = await query
            .AsNoTracking()
            .Where(o => o.SewingTeamId == teamId &&
                        o.CurrentStatus == Domain.Enums.ProductionStatus.Completed &&
                        o.CompletedAt >= startDate && o.CompletedAt <= endDate)
            .ToListAsync();

        if (!teamOrders.Any())
        {
            return new BonusReportDto
            {
                TeamId = teamId,
                TeamName = team.Name,
                FinalBonusPercentage = 0,
                TotalAmount = 0,
                CompletedOrders = 0
            };
        }

        int totalProduced = teamOrders.Sum(o => o.Quantity);
        int onTimeOrders = teamOrders.Count(o => o.CompletedAt <= o.EstimatedCompletionAt);

        // Sum defects from QA Service
        int totalDefects = 0;
        foreach (var order in teamOrders)
        {
            var defects = await _qaService.GetDefectsByOrderAsync(order.Id);
            totalDefects += defects.Sum(d => d.Quantity);
        }

        // --- CALCULATIONS ---

        // 1. Productivity (Based on meta if available, else standard bonus if they produced anything)
        decimal productivityBonus = (decimal)rule.ProductivityPercentage;

        // 2. Deadline Performance
        decimal onTimeRatio = (decimal)onTimeOrders / teamOrders.Count;
        decimal deadlineBonus = onTimeRatio * rule.DeadlineBonusPercentage;

        // 3. Quality Penalty
        decimal defectRatio = totalProduced > 0 ? (decimal)totalDefects / totalProduced * 100 : 0;
        decimal finalBonus = productivityBonus + deadlineBonus;

        // Simple placeholder logic as BonusRule entity might need update to support full properties
        if (defectRatio > 5) // 5% limit
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
            TotalAmount = 0, // Would need a base salary reference here
            CompletedOrders = teamOrders.Count,
            OnTimeOrders = onTimeOrders,
            TotalProduced = totalProduced,
            TotalDefects = totalDefects,
            Orders = teamOrders.Select(o => new OrderBonusDetail
            {
                LotCode = o.LotCode,
                IsOnTime = o.CompletedAt <= o.EstimatedCompletionAt,
                Defects = 0, // Could be detailed per order if needed
                Contribution = Math.Round(finalBonus / teamOrders.Count, 2)
            }).ToList()
        };
    }

    public async Task<BonusReportDto> CalculateUserBonusAsync(int userId, DateTime startDate, DateTime endDate)
    {
        var user = await _teamRepo.GetMemberByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        var rule = await _ruleRepo.GetActiveRuleAsync() ?? new BonusRule();
        var query = await _orderRepo.GetQueryableAsync();

        // 1. Get Individual Orders
        var userOrders = await query
            .AsNoTracking()
            .Where(o => o.UserId == userId &&
                        o.CurrentStatus == Domain.Enums.ProductionStatus.Completed &&
                        o.CompletedAt >= startDate && o.CompletedAt <= endDate)
            .ToListAsync();

        int totalProduced = userOrders.Sum(o => o.Quantity);
        int onTimeOrders = userOrders.Count(o => o.CompletedAt <= o.EstimatedCompletionAt);

        int totalDefects = 0;
        foreach (var order in userOrders)
        {
            var defects = await _qaService.GetDefectsByOrderAsync(order.Id);
            totalDefects += defects.Sum(d => d.Quantity);
        }

        decimal productivityBonus = userOrders.Any() ? (decimal)rule.ProductivityPercentage : 0;
        decimal onTimeRatio = userOrders.Any() ? (decimal)onTimeOrders / userOrders.Count : 0;
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
            TeamName = user.FullName, // Using TeamName field for the User's name
            ProductivityPercentage = individualBonus,
            DeadlinePerformance = Math.Round(onTimeRatio * 100, 2),
            DefectPercentage = Math.Round(defectRatio, 2),
            FinalBonusPercentage = Math.Round(finalBonus, 2),
            CompletedOrders = userOrders.Count,
            TotalProduced = totalProduced,
            TotalDefects = totalDefects
        };
    }
}
