using GestionProduccion.Domain.Entities;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionProduccion.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SewingTeamsController : ControllerBase
{
    private readonly ISewingTeamRepository _teamRepo;

    public SewingTeamsController(ISewingTeamRepository teamRepo)
    {
        _teamRepo = teamRepo;
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
            MemberCount = t.Members.Count
        }).ToList();

        return Ok(new ApiResponse<List<SewingTeamDto>> { Success = true, Data = dtos });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SewingTeamDto>>> GetById(int id)
    {
        var team = await _teamRepo.GetTeamWithMembersAsync(id);
        if (team == null) return NotFound();

        var dto = new SewingTeamDto
        {
            Id = team.Id,
            Name = team.Name,
            IsActive = team.IsActive,
            Members = team.Members.Select(m => new UserDto
            {
                Id = m.Id,
                FullName = m.FullName,
                Email = m.Email,
                Role = m.Role
            }).ToList()
        };

        return Ok(new ApiResponse<SewingTeamDto> { Success = true, Data = dto });
    }

    [HttpPost]
    public async Task<IActionResult> Create(SewingTeamDto dto)
    {
        var team = new SewingTeam
        {
            Name = dto.Name,
            IsActive = true
        };
        await _teamRepo.AddAsync(team);
        await _teamRepo.SaveChangesAsync();
        return Ok(new ApiResponse<int> { Success = true, Data = team.Id });
    }
}
