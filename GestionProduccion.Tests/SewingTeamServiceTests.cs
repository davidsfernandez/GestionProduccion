using FluentAssertions;
using GestionProduccion.Data;
using GestionProduccion.Data.Repositories;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Exceptions;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestionProduccion.Tests;

public class SewingTeamServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly SewingTeamService _service;
    private readonly SewingTeamRepository _teamRepo;
    private readonly UserRepository _userRepo;
    private readonly ProductionOrderRepository _orderRepo;

    public SewingTeamServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        
        _teamRepo = new SewingTeamRepository(_context);
        _userRepo = new UserRepository(_context);
        _orderRepo = new ProductionOrderRepository(_context);
        
        _service = new SewingTeamService(_teamRepo, _userRepo, _orderRepo);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task UpdateTeamAsync_ShouldRemoveTeamId_FromUnselectedUsers()
    {
        // Arrange
        var team = new SewingTeam { Name = "Team Alpha", IsActive = true };
        _context.SewingTeams.Add(team);
        await _context.SaveChangesAsync();

        var user1 = new User { Id = 1, FullName = "User 1", Email = "u1@test.com", Role = UserRole.Operational, SewingTeamId = team.Id };
        var user2 = new User { Id = 2, FullName = "User 2", Email = "u2@test.com", Role = UserRole.Operational, SewingTeamId = team.Id };
        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        var updateDto = new SewingTeamDto 
        { 
            Id = team.Id, 
            Name = "Team Alpha Updated", 
            IsActive = true,
            SelectedUserIds = new List<int> { 1 } // Only keep user 1
        };

        // Act
        await _service.UpdateTeamAsync(team.Id, updateDto);

        // Assert
        var dbUser2 = await _context.Users.FindAsync(2);
        dbUser2!.SewingTeamId.Should().BeNull();
        
        var dbUser1 = await _context.Users.FindAsync(1);
        dbUser1!.SewingTeamId.Should().Be(team.Id);
    }

    [Fact]
    public async Task UpdateTeamAsync_ShouldAssignTeamId_ToNewlySelectedUsers()
    {
        // Arrange
        var team = new SewingTeam { Name = "Team Beta", IsActive = true };
        _context.SewingTeams.Add(team);
        await _context.SaveChangesAsync();

        var user1 = new User { Id = 1, FullName = "User 1", Email = "u1@test.com", Role = UserRole.Operational, SewingTeamId = null };
        var user2 = new User { Id = 2, FullName = "User 2", Email = "u2@test.com", Role = UserRole.Operational, SewingTeamId = null };
        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        var updateDto = new SewingTeamDto 
        { 
            Id = team.Id, 
            Name = "Team Beta", 
            IsActive = true,
            SelectedUserIds = new List<int> { 1, 2 } 
        };

        // Act
        await _service.UpdateTeamAsync(team.Id, updateDto);

        // Assert
        var dbUser1 = await _context.Users.FindAsync(1);
        var dbUser2 = await _context.Users.FindAsync(2);
        dbUser1!.SewingTeamId.Should().Be(team.Id);
        dbUser2!.SewingTeamId.Should().Be(team.Id);
    }

    [Fact]
    public async Task CreateTeamAsync_ShouldThrowException_WhenNoMembersProvided()
    {
        // Arrange
        var request = new CreateSewingTeamRequest 
        { 
            Name = "Empty Team", 
            InitialUserIds = new List<int>() // Empty
        };

        // Act
        Func<Task> act = async () => await _service.CreateTeamAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainConstraintException>()
            .WithMessage("A team must have at least one user assigned upon creation.");
    }

    [Fact]
    public async Task DeleteTeamAsync_ShouldTriggerWorkloadBalancing_WhenTeamHasMembers()
    {
        // Arrange
        var teamA = new SewingTeam { Id = 1, Name = "Team A", IsActive = true };
        var teamB = new SewingTeam { Id = 2, Name = "Team B", IsActive = true };
        _context.SewingTeams.AddRange(teamA, teamB);
        
        var userA = new User { Id = 1, FullName = "User A", Email = "a@test.com", Role = UserRole.Operational, SewingTeamId = 1 };
        _context.Users.Add(userA);
        
        await _context.SaveChangesAsync();

        // Act
        await _service.DeleteTeamAsync(1);

        // Assert
        var dbUserA = await _context.Users.FindAsync(1);
        dbUserA!.SewingTeamId.Should().Be(2); // Should be reassigned to Team B
    }
}
