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
        if (team == null) throw new KeyNotFoundException("Equipe não encontrada.");

        var rule = await _ruleRepo.GetActiveRuleAsync() ?? new BonusRule();
        
        // Query completed orders within range for this team
        var orders = await _orderRepo.GetAllAsync();
        var teamOrders = orders.Where(o => o.SewingTeamId == teamId && 
                                           o.CurrentStatus == Domain.Enums.ProductionStatus.Completed &&
                                           o.ActualEndDate >= startDate && o.ActualEndDate <= endDate).ToList();

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
        int onTimeOrders = teamOrders.Count(o => o.ActualEndDate <= o.EstimatedDeliveryDate);
        
        // Sum defects from QA Service
        int totalDefects = 0;
        foreach(var order in teamOrders)
        {
            var defects = await _qaService.GetDefectsByOrderAsync(order.Id);
            totalDefects += defects.Sum(d => d.Quantity);
        }

        // --- CALCULATIONS ---
        
        // 1. Productivity (Based on meta if available, else standard bonus if they produced anything)
        decimal productivityBonus = rule.ProductivityPercentage; // Simplified: constant if criteria met

        // 2. Deadline Performance
        decimal onTimeRatio = (decimal)onTimeOrders / teamOrders.Count;
        decimal deadlineBonus = onTimeRatio * rule.DeadlineBonusPercentage;
        
        // 3. Quality Penalty
        decimal defectRatio = totalProduced > 0 ? (decimal)totalDefects / totalProduced * 100 : 0;
        decimal finalBonus = productivityBonus + deadlineBonus;

        if (defectRatio > rule.DefectLimitPercentage)
        {
            finalBonus = 0; // Strict quality gate
        }
        else if (onTimeRatio < 0.5m)
        {
            finalBonus -= rule.DelayPenaltyPercentage; // Penalty for low delivery speed
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
                UniqueCode = o.UniqueCode,
                IsOnTime = o.ActualEndDate <= o.EstimatedDeliveryDate,
                Defects = 0, // Could be detailed per order if needed
                Contribution = Math.Round(finalBonus / teamOrders.Count, 2)
            }).ToList()
        };
    }
}
