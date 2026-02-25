using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Exceptions;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestionProduccion.Services;

public class SewingTeamService : ISewingTeamService
{
    private readonly ISewingTeamRepository _teamRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductionOrderRepository _orderRepository;

    public SewingTeamService(
        ISewingTeamRepository teamRepository,
        IUserRepository userRepository,
        IProductionOrderRepository orderRepository)
    {
        _teamRepository = teamRepository;
        _userRepository = userRepository;
        _orderRepository = orderRepository;
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

    /// <summary>
    /// Creates a new team and assigns initial members.
    /// </summary>
    public async Task<SewingTeamDto> CreateTeamAsync(CreateSewingTeamRequest request)
    {
        // 1. Validation: List not empty (Fase 1: 9)
        if (request.InitialUserIds == null || !request.InitialUserIds.Any())
        {
            throw new DomainConstraintException("A team must have at least one user assigned upon creation.");
        }

        // 2. Validation: Eligible staff exists (Fase 1: 10-11)
        var allUsers = await _userRepository.GetAllActiveAsync();
        var eligibleUsersCount = allUsers.Count(u => u.Role == UserRole.Leader || u.Role == UserRole.Operational);

        if (eligibleUsersCount == 0)
        {
            throw new DomainConstraintException("Cannot create a team. No leaders or operational staff exist in the system.");
        }

        // 3. Check for name uniqueness
        var existingTeams = await _teamRepository.GetAllAsync();
        if (existingTeams.Any(t => t.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainConstraintException("A team with this name already exists.");
        }

        // 4. Create the team
        var team = new SewingTeam
        {
            Name = request.Name,
            IsActive = true
        };

        await _teamRepository.AddAsync(team);
        await _teamRepository.SaveChangesAsync(); // Get the ID

        // 5. Assign users (Fase 1: 12-13)
        foreach (var userId in request.InitialUserIds)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null && (user.Role == UserRole.Leader || user.Role == UserRole.Operational))
            {
                user.SewingTeamId = team.Id;
                await _userRepository.UpdateAsync(user);
            }
        }

        await _userRepository.SaveChangesAsync();

        // Refetch to get members
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

        // Synchronize Members
        var currentMemberIds = team.Members.Select(m => m.Id).ToList();
        
        // 1. Members to remove (Existing in DB but NOT in SelectedUserIds)
        var toRemoveIds = currentMemberIds.Except(dto.SelectedUserIds).ToList();
        foreach (var userId in toRemoveIds)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.SewingTeamId = null;
                await _userRepository.UpdateAsync(user);
            }
        }

        // 2. Members to add (New in SelectedUserIds but NOT in DB)
        var toAddIds = dto.SelectedUserIds.Except(currentMemberIds).ToList();
        foreach (var userId in toAddIds)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null && (user.Role == UserRole.Leader || user.Role == UserRole.Operational))
            {
                user.SewingTeamId = id;
                await _userRepository.UpdateAsync(user);
            }
        }

        await _teamRepository.UpdateAsync(team);
        await _teamRepository.SaveChangesAsync();
        await _userRepository.SaveChangesAsync();

        var updatedTeam = await _teamRepository.GetTeamWithMembersAsync(id);
        return MapToDto(updatedTeam!);
    }

    /// <summary>
    /// Deletes a team and reassigns its members using a Round-Robin algorithm based on workload.
    /// </summary>
    public async Task<bool> DeleteTeamAsync(int id)
    {
        // 1. Recover team with members (Fase 2: 81-82)
        var teamToDelete = await _teamRepository.GetTeamWithMembersAsync(id);
        if (teamToDelete == null) return false;

        var orphanUsers = teamToDelete.Members.ToList();

        if (orphanUsers.Any())
        {
            // 2. Find other active teams (Fase 2: 84)
            var otherTeams = (await _teamRepository.GetAllAsync())
                .Where(t => t.Id != id && t.IsActive)
                .ToList();

            if (!otherTeams.Any())
            {
                // Fase 2: 85
                throw new DomainConstraintException("Cannot delete the only existing team because there are users that cannot be reassigned.");
            }

            // 3. Build Workload Metric (Fase 2: 86-87)
            var queryableOrders = await _orderRepository.GetQueryableAsync();
            var activeOrders = await queryableOrders
                .Where(o => o.CurrentStatus == ProductionStatus.InProduction || o.CurrentStatus == ProductionStatus.Pending)
                .ToListAsync();

            var teamWorkloads = otherTeams.Select(t => new
            {
                Team = t,
                Workload = activeOrders.Count(o => o.SewingTeamId == t.Id)
            })
            .OrderBy(tw => tw.Workload) // Least workload first
            .ToList();

            var sortedTeams = teamWorkloads.Select(tw => tw.Team).ToList();

            // 4. Round-Robin Distribution (Fase 2: 89-94)
            int teamIndex = 0;
            foreach (var user in orphanUsers)
            {
                user.SewingTeamId = sortedTeams[teamIndex].Id;
                await _userRepository.UpdateAsync(user);

                teamIndex = (teamIndex + 1) % sortedTeams.Count;
            }
        }

        // 5. Finalize deletion (Soft Delete if orders exist, else Physical Delete)
        var queryableOrders = await _orderRepository.GetQueryableAsync();
        bool hasOrders = await queryableOrders.AnyAsync(o => o.SewingTeamId == id);

        if (hasOrders)
        {
            // If team has historical or active orders, we cannot physically delete it due to FK RESTRICT.
            // We perform a Soft Delete by marking it inactive.
            teamToDelete.IsActive = false;
            await _teamRepository.UpdateAsync(teamToDelete);
        }
        else
        {
            // No orders linked, we can safely delete the record.
            await _teamRepository.DeleteAsync(teamToDelete);
        }

        await _teamRepository.SaveChangesAsync();
        await _userRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ToggleTeamStatusAsync(int id)
    {
        var team = await _teamRepository.GetByIdAsync(id);
        if (team == null) return false;

        team.IsActive = !team.IsActive;
        await _teamRepository.UpdateAsync(team);
        await _teamRepository.SaveChangesAsync();
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
