using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Controllers;

[Authorize(Roles = "Administrator")]
[ApiController]
[Route("api/[controller]")]
public class SewingTeamsController : ControllerBase
{
    private readonly ISewingTeamRepository _teamRepo;
    private readonly IUserRepository _userRepo;

    public SewingTeamsController(ISewingTeamRepository teamRepo, IUserRepository userRepo)
    {
        _teamRepo = teamRepo;
        _userRepo = userRepo;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SewingTeamDto>>>> GetAll()
    {
        var teams = await _teamRepo.GetAllAsync();
        var dtos = teams.Select(t => new SewingTeamDto
        {
            Id = t.Id,
            Name = t.Name,
            IsActive = t.IsActive,
            MemberCount = 0 // Needs Include for detail
        }).ToList();

        return Ok(new ApiResponse<List<SewingTeamDto>> { Success = true, Data = dtos });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SewingTeamDto>>> GetById(int id)
    {
        var team = await _teamRepo.GetTeamWithMembersAsync(id);
        if (team == null) return NotFound(new ApiResponse<SewingTeamDto> { Success = false, Message = "Equipe não encontrada" });

        var dto = new SewingTeamDto
        {
            Id = team.Id,
            Name = team.Name,
            IsActive = team.IsActive,
            Members = team.Members.Select(m => new UserSummaryDto { Id = m.Id, Name = m.Name }).ToList(),
            MemberCount = team.Members.Count
        };

        return Ok(new ApiResponse<SewingTeamDto> { Success = true, Data = dto });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SewingTeamDto>>> Create(CreateSewingTeamDto dto)
    {
        var team = new SewingTeam
        {
            Name = dto.Name,
            IsActive = true,
            CreationDate = DateTime.UtcNow
        };

        if (dto.MemberIds.Any())
        {
            foreach (var userId in dto.MemberIds)
            {
                var user = await _userRepo.GetByIdAsync(userId);
                if (user != null) team.Members.Add(user);
            }
        }

        await _teamRepo.AddAsync(team);
        await _teamRepo.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = team.Id }, new ApiResponse<SewingTeamDto> { Success = true, Message = "Equipe criada com sucesso" });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Update(int id, CreateSewingTeamDto dto)
    {
        var team = await _teamRepo.GetTeamWithMembersAsync(id);
        if (team == null) return NotFound(new ApiResponse<bool> { Success = false, Message = "Equipe não encontrada" });

        team.Name = dto.Name;
        
        // Update members
        team.Members.Clear();
        foreach (var userId in dto.MemberIds)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user != null) team.Members.Add(user);
        }

        await _teamRepo.UpdateAsync(team);
        await _teamRepo.SaveChangesAsync();

        return Ok(new ApiResponse<bool> { Success = true, Message = "Equipe atualizada" });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleActive(int id)
    {
        var team = await _teamRepo.GetByIdAsync(id);
        if (team == null) return NotFound(new ApiResponse<bool> { Success = false });

        team.IsActive = !team.IsActive;
        await _teamRepo.UpdateAsync(team);
        await _teamRepo.SaveChangesAsync();

        return Ok(new ApiResponse<bool> { Success = true, Message = team.IsActive ? "Equipe ativada" : "Equipe desativada" });
    }
}
