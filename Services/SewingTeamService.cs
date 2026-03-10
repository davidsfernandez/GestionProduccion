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
using GestionProduccion.Domain.Exceptions;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using GestionProduccion.Hubs;

namespace GestionProduccion.Services;

public class SewingTeamService : ISewingTeamService
{
    private readonly ISewingTeamRepository _teamRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductionOrderRepository _orderRepository;
    private readonly IHubContext<ProductionHub> _hubContext;

    public SewingTeamService(
        ISewingTeamRepository teamRepository,
        IUserRepository userRepository,
        IProductionOrderRepository orderRepository,
        IHubContext<ProductionHub> hubContext)
    {
        _teamRepository = teamRepository;
        _userRepository = userRepository;
        _orderRepository = orderRepository;
        _hubContext = hubContext;
    }

    public async Task<List<SewingTeamDto>> GetAllTeamsAsync()
    {
        var teams = await _teamRepository.GetAllAsync();
        return teams.Select(MapToDto).ToList();
    }

    public async Task<SewingTeamDto?> GetTeamByIdAsync(int id)
    {
        var team = await _teamRepository.GetTeamWithMembersAsync(id);
        return team == null ? null : MapToDto(team);
    }

    public async Task<SewingTeamDto> CreateTeamAsync(CreateSewingTeamRequest request)
    {
        if (request.InitialUserIds == null || !request.InitialUserIds.Any())
        {
            throw new DomainConstraintException("A team must have at least one user assigned upon creation.");
        }

        var allUsers = await _userRepository.GetAllActiveAsync();
        var eligibleUsersCount = allUsers.Count(u => u.Role == UserRole.Leader || u.Role == UserRole.Operational);

        if (eligibleUsersCount == 0)
        {
            throw new DomainConstraintException("Cannot create a team. No eligible staff exist.");
        }

        var existingTeams = await _teamRepository.GetAllAsync();
        if (existingTeams.Any(t => t.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainConstraintException("A team with this name already exists.");
        }

        var team = new SewingTeam { Name = request.Name, IsActive = true };
        await _teamRepository.AddAsync(team);
        await _teamRepository.SaveChangesAsync();

        foreach (var userId in request.InitialUserIds)
        {
            await AddMemberAsync(team.Id, userId);
        }

        await _hubContext.Clients.All.SendAsync("ReceiveMessage", new { 
            message = $"Nova equipe criada: {team.Name} 🚀", 
            type = "TeamCreated" 
        });

        var createdTeam = await _teamRepository.GetTeamWithMembersAsync(team.Id);
        return MapToDto(createdTeam!);
    }

    public async Task<SewingTeamDto> UpdateTeamAsync(int id, SewingTeamDto dto)
    {
        var team = await _teamRepository.GetTeamWithMembersAsync(id);
        if (team == null) throw new KeyNotFoundException("Team not found.");

        if (dto.SelectedUserIds == null || !dto.SelectedUserIds.Any())
        {
            throw new DomainConstraintException("A team must have at least one member.");
        }

        team.Name = dto.Name;
        team.IsActive = dto.IsActive;

        var currentMemberIds = team.Members.Select(m => m.Id).ToList();
        var toRemoveIds = currentMemberIds.Except(dto.SelectedUserIds).ToList();
        var toAddIds = dto.SelectedUserIds.Except(currentMemberIds).ToList();

        foreach (var userId in toRemoveIds) await RemoveMemberAsync(id, userId);
        foreach (var userId in toAddIds) await AddMemberAsync(id, userId);

        await _teamRepository.UpdateAsync(team);
        await _teamRepository.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("ReceiveMessage", new { 
            message = $"Equipe {team.Name} atualizada.", 
            type = "TeamUpdated" 
        });

        var updatedTeam = await _teamRepository.GetTeamWithMembersAsync(id);
        return MapToDto(updatedTeam!);
    }

    public async Task AddMemberAsync(int teamId, int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null && (user.Role == UserRole.Leader || user.Role == UserRole.Operational))
        {
            var previousTeamId = user.SewingTeamId;
            user.SewingTeamId = teamId;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveNotification", userId, "Mudança de Equipe", $"Você foi atribuído à equipe ID: {teamId}");
        }
    }

    public async Task RemoveMemberAsync(int teamId, int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null && user.SewingTeamId == teamId)
        {
            user.SewingTeamId = null;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveNotification", userId, "Saída de Equipe", "Você foi removido da sua equipe atual.");
        }
    }

    public async Task<bool> DeleteTeamAsync(int id)
    {
        var teamToDelete = await _teamRepository.GetTeamWithMembersAsync(id);
        if (teamToDelete == null) return false;

        var orphanUsers = teamToDelete.Members.ToList();

        if (orphanUsers.Any())
        {
            var otherTeams = (await _teamRepository.GetAllAsync()).Where(t => t.Id != id && t.IsActive).ToList();
            if (!otherTeams.Any()) throw new DomainConstraintException("Cannot delete team. No other active teams for reassignment.");

            var queryableOrders = await _orderRepository.GetQueryableAsync();
            var activeOrders = await queryableOrders
                .Where(o => o.CurrentStatus == ProductionStatus.InProduction || o.CurrentStatus == ProductionStatus.Pending)
                .ToListAsync();

            var sortedTeams = otherTeams.Select(t => new { Team = t, Workload = activeOrders.Count(o => o.SewingTeamId == t.Id) })
                .OrderBy(tw => tw.Workload).Select(tw => tw.Team).ToList();

            int teamIndex = 0;
            foreach (var user in orphanUsers)
            {
                var targetTeam = sortedTeams[teamIndex];
                await AddMemberAsync(targetTeam.Id, user.Id);
                teamIndex = (teamIndex + 1) % sortedTeams.Count;
            }
        }

        var checkOrdersQuery = await _orderRepository.GetQueryableAsync();
        bool hasOrders = await checkOrdersQuery.AnyAsync(o => o.SewingTeamId == id);

        if (hasOrders)
        {
            teamToDelete.IsActive = false;
            await _teamRepository.UpdateAsync(teamToDelete);
        }
        else
        {
            await _teamRepository.DeleteAsync(teamToDelete);
        }

        await _teamRepository.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", new { message = $"Equipe {teamToDelete.Name} removida/desativada.", type = "TeamDeleted" });

        return true;
    }

    public async Task<bool> ToggleTeamStatusAsync(int id)
    {
        var team = await _teamRepository.GetByIdAsync(id);
        if (team == null) return false;

        team.IsActive = !team.IsActive;
        await _teamRepository.UpdateAsync(team);
        await _teamRepository.SaveChangesAsync();
        
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", new { 
            message = $"Status da equipe {team.Name} alterado para {(team.IsActive ? "Ativo" : "Inativo")}.", 
            type = "TeamStatusChanged" 
        });
        
        return true;
    }

    private SewingTeamDto MapToDto(SewingTeam team)
    {
        return new SewingTeamDto
        {
            Id = team.Id,
            Name = team.Name,
            IsActive = team.IsActive,
            MemberCount = team.Members?.Count ?? 0,
            Members = team.Members?.Select(m => new UserDto
            {
                Id = m.Id,
                ExternalId = m.ExternalId,
                FullName = m.FullName,
                Email = m.Email,
                Role = m.Role,
                SewingTeamId = m.SewingTeamId,
                SewingTeamName = team.Name
            }).ToList() ?? new List<UserDto>()
        };
    }
}
