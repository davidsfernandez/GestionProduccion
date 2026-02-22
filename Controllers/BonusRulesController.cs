using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Controllers;

[Authorize(Roles = "Administrator")]
[ApiController]
[Route("api/[controller]")]
public class BonusRulesController : ControllerBase
{
    private readonly IBonusRuleRepository _ruleRepo;

    public BonusRulesController(IBonusRuleRepository ruleRepo)
    {
        _ruleRepo = ruleRepo;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<BonusRuleDto>>> GetActive()
    {
        var rule = await _ruleRepo.GetActiveRuleAsync();
        if (rule == null)
        {
            // Seed default if none
            rule = new BonusRule
            {
                ProductivityPercentage = 10,
                DeadlineBonusPercentage = 5,
                DefectLimitPercentage = 2,
                DelayPenaltyPercentage = 5,
                IsActive = true
            };
            await _ruleRepo.AddAsync(rule);
            await _ruleRepo.SaveChangesAsync();
        }

        var dto = new BonusRuleDto
        {
            Id = rule.Id,
            ProductivityPercentage = rule.ProductivityPercentage,
            DeadlineBonusPercentage = rule.DeadlineBonusPercentage,
            DefectLimitPercentage = rule.DefectLimitPercentage,
            DelayPenaltyPercentage = rule.DelayPenaltyPercentage,
            LastUpdate = rule.LastUpdate
        };

        return Ok(new ApiResponse<BonusRuleDto> { Success = true, Data = dto });
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<bool>>> Update(UpdateBonusRuleDto dto)
    {
        var rule = await _ruleRepo.GetActiveRuleAsync();
        if (rule == null) return BadRequest(new ApiResponse<bool> { Success = false });

        rule.ProductivityPercentage = dto.ProductivityPercentage;
        rule.DeadlineBonusPercentage = dto.DeadlineBonusPercentage;
        rule.DefectLimitPercentage = dto.DefectLimitPercentage;
        rule.DelayPenaltyPercentage = dto.DelayPenaltyPercentage;
        rule.LastUpdate = DateTime.UtcNow;

        await _ruleRepo.UpdateAsync(rule);
        await _ruleRepo.SaveChangesAsync();

        return Ok(new ApiResponse<bool> { Success = true, Message = "Regras atualizadas com sucesso" });
    }
}
