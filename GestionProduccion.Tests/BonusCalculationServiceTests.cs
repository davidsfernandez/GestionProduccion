/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GestionProduccion.Application.Mappers;

namespace GestionProduccion.Tests;

public class BonusCalculationServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly BonusCalculationService _service;
    private readonly Mock<IBonusRuleRepository> _mockRuleRepo;
    private readonly Mock<IQAService> _mockQaService;
    private readonly SewingTeamRepository _teamRepo;
    private readonly ProductionOrderRepository _orderRepo;
    private readonly ProductRepository _productRepo;
    private readonly ProductionOrderOutputRepository _outputRepo;
    private readonly MainMapper _mapper;

    public BonusCalculationServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _teamRepo = new SewingTeamRepository(_context);
        _orderRepo = new ProductionOrderRepository(_context);
        _productRepo = new ProductRepository(_context);
        _outputRepo = new ProductionOrderOutputRepository(_context);
        _mockRuleRepo = new Mock<IBonusRuleRepository>();
        _mockQaService = new Mock<IQAService>();
        _mapper = new MainMapper();

        _service = new BonusCalculationService(
            _teamRepo, 
            _orderRepo, 
            _productRepo,
            _mockRuleRepo.Object, 
            _mockQaService.Object, 
            _outputRepo, 
            _mapper);
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
        var product = new Product { Id = 1, Name = "P1", InternalCode = "C1", MainSku = "S1", FabricType = "F1", AverageProductionTimeMinutes = 10 };
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
            ProductId = 1,
            EffectiveMinutes = 1000 // Standard is 10 min * 100 qty = 1000 min (100% efficiency)
        };
        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync();

        var rule = new BonusRule { ProductivityPercentage = 100.0, DeadlineBonusPercentage = 20.0m };
        _mockRuleRepo.Setup(r => r.GetActiveRuleAsync()).ReturnsAsync(rule);
        _mockQaService.Setup(qa => qa.GetDefectsByOrderAsync(It.IsAny<int>())).ReturnsAsync(new List<QADefectDto>());

        // Act
        var result = await _service.CalculateTeamBonusAsync(1, fixedDate.AddDays(-10), fixedDate.AddDays(10));

        // Assert
        // 100 (Base) * 1.0 (Eff) + 20 (Deadline) = 120
        result.FinalBonusPercentage.Should().Be(120m);
    }

    [Fact]
    public async Task CalculateTeamBonus_ShouldZeroOutBonus_WhenDefectsExceedThreshold()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "P1", InternalCode = "C1", MainSku = "S1", FabricType = "F1", AverageProductionTimeMinutes = 10 };
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
            ProductId = 1,
            EffectiveMinutes = 1000
        };
        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync();

        var rule = new BonusRule { ProductivityPercentage = 100.0, DeadlineBonusPercentage = 20.0m, DefectLimitPercentage = 5.0m };
        _mockRuleRepo.Setup(r => r.GetActiveRuleAsync()).ReturnsAsync(rule);

        // 6 defects on 100 items = 6% > 5% threshold
        var defects = new List<QADefectDto> { new QADefectDto { Quantity = 6, ReportedAt = fixedDate } };
        _mockQaService.Setup(qa => qa.GetDefectsByOrderAsync(2)).ReturnsAsync(defects);

        // Act
        var result = await _service.CalculateTeamBonusAsync(2, fixedDate.AddDays(-10), fixedDate.AddDays(10));

        // Assert
        result.FinalBonusPercentage.Should().Be(0m);
    }
}
