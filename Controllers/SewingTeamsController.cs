using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GestionProduccion.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SewingTeamsController : ControllerBase
{
    private readonly ISewingTeamService _teamService;

    public SewingTeamsController(ISewingTeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SewingTeamDto>>>> GetAll()
    {
        var teams = await _teamService.GetAllTeamsAsync();
        return Ok(new ApiResponse<List<SewingTeamDto>> { Success = true, Data = teams });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SewingTeamDto>>> GetById(int id)
    {
        var team = await _teamService.GetTeamByIdAsync(id);
        if (team == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Equipe não encontrada." });

        return Ok(new ApiResponse<SewingTeamDto> { Success = true, Data = team });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSewingTeamRequest request)
    {
        try
        {
            var createdTeam = await _teamService.CreateTeamAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = createdTeam.Id }, new ApiResponse<SewingTeamDto> { Success = true, Data = createdTeam });
        }
        catch (GestionProduccion.Domain.Exceptions.DomainConstraintException ex)
        {
            return BadRequest(new ApiResponse<string> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Erro interno ao criar equipe.", Data = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, SewingTeamDto dto)
    {
        try
        {
            var updatedTeam = await _teamService.UpdateTeamAsync(id, dto);
            return Ok(new ApiResponse<SewingTeamDto> { Success = true, Data = updatedTeam });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse<string> { Success = false, Message = "Equipe não encontrada." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Erro ao atualizar equipe.", Data = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _teamService.DeleteTeamAsync(id);
            if (!result) return NotFound(new ApiResponse<string> { Success = false, Message = "Equipe no encontrada." });

            return Ok(new ApiResponse<bool> { Success = true, Data = true });
        }
        catch (GestionProduccion.Domain.Exceptions.DomainConstraintException ex)
        {
            return Conflict(new ApiResponse<string> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string> { Success = false, Message = "Erro ao excluir equipe.", Data = ex.Message });
        }
    }

    [HttpPatch("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var result = await _teamService.ToggleTeamStatusAsync(id);
        if (!result) return NotFound(new ApiResponse<string> { Success = false, Message = "Equipe não encontrada." });

        return Ok(new ApiResponse<bool> { Success = true, Data = true });
    }
}
