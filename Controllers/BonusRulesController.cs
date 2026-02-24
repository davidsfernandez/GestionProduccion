using GestionProduccion.Domain.Entities;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GestionProduccion.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BonusRulesController : ControllerBase
{
    private readonly IBonusRuleRepository _ruleRepo;

    public BonusRulesController(IBonusRuleRepository ruleRepo)
    {
        _ruleRepo = ruleRepo;
    }

    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<BonusRuleDto>>> GetActive()
    {
        var rule = await _ruleRepo.GetActiveRuleAsync();
        if (rule == null)
        {
            // Default rule if none exists
            rule = new BonusRule
            {
                Name = "Default Rules",
                ProductivityPercentage = 10,
                DeadlineBonusPercentage = 5,
                DefectLimitPercentage = 2,
                DelayPenaltyPercentage = 5,
                IsActive = true
            };
        }

        var dto = new BonusRuleDto
        {
            Id = rule.Id,
            ProductivityPercentage = (decimal)rule.ProductivityPercentage,
            DeadlineBonusPercentage = rule.DeadlineBonusPercentage,
            DefectLimitPercentage = rule.DefectLimitPercentage,
            DelayPenaltyPercentage = rule.DelayPenaltyPercentage,
            LastUpdate = rule.UpdatedAt
        };

        return Ok(new ApiResponse<BonusRuleDto> { Success = true, Data = dto });
    }

    [HttpPost]
    public async Task<IActionResult> Update(BonusRuleDto dto)
    {
        var existing = await _ruleRepo.GetActiveRuleAsync();
        if (existing == null)
        {
            existing = new BonusRule();
            await _ruleRepo.AddAsync(existing);
        }

        existing.Name = "Production Rules";
        existing.ProductivityPercentage = (double)dto.ProductivityPercentage;
        existing.DeadlineBonusPercentage = dto.DeadlineBonusPercentage;
        existing.DefectLimitPercentage = dto.DefectLimitPercentage;
        existing.DelayPenaltyPercentage = dto.DelayPenaltyPercentage;
        existing.UpdatedAt = DateTime.UtcNow;

        await _ruleRepo.UpdateAsync(existing);
        await _ruleRepo.SaveChangesAsync();

        return Ok(new ApiResponse<bool> { Success = true });
    }
}
