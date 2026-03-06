/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionProduccion.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
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
        try
        {
            var teams = await _teamService.GetAllTeamsAsync();
            return Ok(ApiResponse<List<SewingTeamDto>>.SuccessResult(teams));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<SewingTeamDto>>.FailureResult("Error retrieving teams", new List<string> { ex.Message }));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SewingTeamDto>>> GetById(int id)
    {
        try
        {
            var team = await _teamService.GetTeamByIdAsync(id);
            if (team == null) return NotFound(ApiResponse<SewingTeamDto>.FailureResult("Equipe não encontrada."));

            return Ok(ApiResponse<SewingTeamDto>.SuccessResult(team));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<SewingTeamDto>.FailureResult("Error retrieving team", new List<string> { ex.Message }));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ApiResponse<SewingTeamDto>>> Create(CreateSewingTeamRequest request)
    {
        try
        {
            var createdTeam = await _teamService.CreateTeamAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = createdTeam.Id }, ApiResponse<SewingTeamDto>.SuccessResult(createdTeam, "Equipe criada com sucesso."));
        }
        catch (GestionProduccion.Domain.Exceptions.DomainConstraintException ex)
        {
            return BadRequest(ApiResponse<SewingTeamDto>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<SewingTeamDto>.FailureResult("Erro interno ao crear equipe.", new List<string> { ex.Message }));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ApiResponse<SewingTeamDto>>> Update(int id, SewingTeamDto dto)
    {
        try
        {
            var updatedTeam = await _teamService.UpdateTeamAsync(id, dto);
            return Ok(ApiResponse<SewingTeamDto>.SuccessResult(updatedTeam, "Equipe atualizada com sucesso."));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<SewingTeamDto>.FailureResult("Equipe não encontrada."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<SewingTeamDto>.FailureResult("Erro ao actualizar equipe.", new List<string> { ex.Message }));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _teamService.DeleteTeamAsync(id);
            if (!result) return NotFound(ApiResponse<bool>.FailureResult("Equipe não encontrada."));

            return Ok(ApiResponse<bool>.SuccessResult(true, "Equipe excluída com sucesso."));
        }
        catch (GestionProduccion.Domain.Exceptions.DomainConstraintException ex)
        {
            return Conflict(ApiResponse<bool>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.FailureResult("Erro ao excluir equipe.", new List<string> { ex.Message }));
        }
    }

    [HttpPatch("{id}/toggle-status")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleStatus(int id)
    {
        try
        {
            var result = await _teamService.ToggleTeamStatusAsync(id);
            if (!result) return NotFound(ApiResponse<bool>.FailureResult("Equipe no encontrada."));

            return Ok(ApiResponse<bool>.SuccessResult(true));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.FailureResult("Error toggling team status", new List<string> { ex.Message }));
        }
    }
}
