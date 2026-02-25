using FluentAssertions;
using GestionProduccion.Data;
using GestionProduccion.Data.Repositories;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GestionProduccion.Tests;

public class BonusCalculationServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly BonusCalculationService _service;
    private readonly Mock<IBonusRuleRepository> _mockRuleRepo;
    private readonly Mock<IQAService> _mockQaService;
    private readonly SewingTeamRepository _teamRepo;
    private readonly ProductionOrderRepository _orderRepo;

    public BonusCalculationServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _teamRepo = new SewingTeamRepository(_context);
        _orderRepo = new ProductionOrderRepository(_context);
        _mockRuleRepo = new Mock<IBonusRuleRepository>();
        _mockQaService = new Mock<IQAService>();

        _service = new BonusCalculationService(_teamRepo, _orderRepo, _mockRuleRepo.Object, _mockQaService.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CalculateTeamBonus_ShouldReturnCorrectAmount_WhenEfficiencyIsHigh()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "P1", InternalCode = "C1", MainSku = "S1", FabricType = "F1" };
        _context.Products.Add(product);

        var fixedDate = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var team = new SewingTeam { Id = 1, Name = "Alpha", IsActive = true };
        _context.SewingTeams.Add(team);

        var order = new ProductionOrder
        {
            Id = 1,
            SewingTeamId = 1,
            Quantity = 100,
            CurrentStatus = ProductionStatus.Completed,
            CompletedAt = fixedDate,
            EstimatedCompletionAt = fixedDate.AddHours(1), // On Time
            ProductId = 1
        };
        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync();

        // 100 base + 20% for deadline
        var rule = new BonusRule { ProductivityPercentage = 100.0, DeadlineBonusPercentage = 20.0m };
        _mockRuleRepo.Setup(r => r.GetActiveRuleAsync()).ReturnsAsync(rule);

        _mockQaService.Setup(qa => qa.GetDefectsByOrderAsync(It.IsAny<int>())).ReturnsAsync(new List<QADefect>());

        // Act
        // Use wide range
        var result = await _service.CalculateTeamBonusAsync(1, fixedDate.AddDays(-10), fixedDate.AddDays(10));

        // Assert
        // 100 (Prod) + 20 (Deadline * 100% ratio) = 120
        result.FinalBonusPercentage.Should().Be(120m);
    }

    [Fact]
    public async Task CalculateTeamBonus_ShouldZeroOutBonus_WhenDefectsExceedThreshold()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "P1", InternalCode = "C1", MainSku = "S1", FabricType = "F1" };
        _context.Products.Add(product);

        var fixedDate = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var team = new SewingTeam { Id = 2, Name = "Beta", IsActive = true };
        _context.SewingTeams.Add(team);

        var order = new ProductionOrder
        {
            Id = 2,
            SewingTeamId = 2,
            Quantity = 100,
            CurrentStatus = ProductionStatus.Completed,
            CompletedAt = fixedDate,
            EstimatedCompletionAt = fixedDate.AddHours(1),
            ProductId = 1
        };
        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync();

        var rule = new BonusRule { ProductivityPercentage = 100.0, DeadlineBonusPercentage = 20.0m };
        _mockRuleRepo.Setup(r => r.GetActiveRuleAsync()).ReturnsAsync(rule);

        // 6 defects on 100 items = 6% > 5% threshold
        var defects = new List<QADefect> { new QADefect { Quantity = 6 } };
        _mockQaService.Setup(qa => qa.GetDefectsByOrderAsync(2)).ReturnsAsync(defects);

        // Act
        var result = await _service.CalculateTeamBonusAsync(2, fixedDate.AddDays(-10), fixedDate.AddDays(10));

        // Assert
        result.FinalBonusPercentage.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateUserBonus_ShouldSumIndividualAndTeamShares()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "P1", InternalCode = "C1", MainSku = "S1", FabricType = "F1" };
        _context.Products.Add(product);

        var fixedDate = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var team = new SewingTeam { Id = 3, Name = "Charlie", IsActive = true };
        _context.SewingTeams.Add(team);

        var user = new User { Id = 1, FullName = "Worker A", SewingTeamId = 3, Role = UserRole.Operational };
        // Add 3 other dummy users to make team size 4
        var user2 = new User { Id = 2, SewingTeamId = 3 };
        var user3 = new User { Id = 3, SewingTeamId = 3 };
        var user4 = new User { Id = 4, SewingTeamId = 3 };
        _context.Users.AddRange(user, user2, user3, user4);

        // Team Order (Bonus Source)
        var teamOrder = new ProductionOrder
        {
            Id = 10,
            SewingTeamId = 3,
            Quantity = 100,
            CurrentStatus = ProductionStatus.Completed,
            CompletedAt = fixedDate,
            EstimatedCompletionAt = fixedDate.AddHours(1),
            ProductId = 1
        };
        // Individual Order
        var userOrder = new ProductionOrder
        {
            Id = 11,
            UserId = 1, // Worker A
            Quantity = 50,
            CurrentStatus = ProductionStatus.Completed,
            CompletedAt = fixedDate,
            EstimatedCompletionAt = fixedDate.AddHours(1),
            ProductId = 1
        };

        _context.ProductionOrders.AddRange(teamOrder, userOrder);
        await _context.SaveChangesAsync();

        // Rule: 100 (Prod) + 0 (Deadline for simplicity) = 100 Total
        var rule = new BonusRule { ProductivityPercentage = 100.0, DeadlineBonusPercentage = 0m };
        _mockRuleRepo.Setup(r => r.GetActiveRuleAsync()).ReturnsAsync(rule);
        _mockQaService.Setup(qa => qa.GetDefectsByOrderAsync(It.IsAny<int>())).ReturnsAsync(new List<QADefect>());

        // Act
        var result = await _service.CalculateUserBonusAsync(1, fixedDate.AddDays(-10), fixedDate.AddDays(10));

        // Assert
        result.FinalBonusPercentage.Should().Be(125m);
    }

    [Fact]
    public async Task CalculateTeamBonus_ShouldReturnZero_WhenNoOrdersCompletedInPeriod()
    {
        // Arrange
        var fixedDate = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var team = new SewingTeam { Id = 4, Name = "Delta" };
        _context.SewingTeams.Add(team);
        await _context.SaveChangesAsync();

        var rule = new BonusRule { ProductivityPercentage = 100.0 };
        _mockRuleRepo.Setup(r => r.GetActiveRuleAsync()).ReturnsAsync(rule);

        // Act
        var result = await _service.CalculateTeamBonusAsync(4, fixedDate.AddDays(-10), fixedDate.AddDays(10));

        // Assert
        result.FinalBonusPercentage.Should().Be(0);
        result.CompletedOrders.Should().Be(0);
    }
}
