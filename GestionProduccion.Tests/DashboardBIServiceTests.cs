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
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestionProduccion.Tests;

public class DashboardBIServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly DashboardBIService _service;

    public DashboardBIServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _service = new DashboardBIService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetCompleteDashboardAsync_ShouldCalculate_DeadStockCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var oldDate = now.AddDays(-100);

        // Product A: No orders (Stalled)
        var productA = new Product { Id = 10, Name = "Product A", MainSku = "SKU-A", InternalCode = "C-A", FabricType = "F" };
        // Product B: Only very old orders (Stalled)
        var productB = new Product { Id = 20, Name = "Product B", MainSku = "SKU-B", InternalCode = "C-B", FabricType = "F" };
        // Product C: Has recent orders (Active)
        var productC = new Product { Id = 30, Name = "Product C", MainSku = "SKU-C", InternalCode = "C-C", FabricType = "F" };

        _context.Products.AddRange(productA, productB, productC);
        await _context.SaveChangesAsync();

        var oldOrder = new ProductionOrder
        {
            LotCode = "OLD",
            ProductId = 20,
            Quantity = 10,
            CreatedAt = oldDate,
            CurrentStatus = ProductionStatus.Completed,
            CurrentStage = ProductionStage.Packaging,
            CompletedAt = oldDate
        };

        var recentOrder = new ProductionOrder
        {
            LotCode = "RECENT",
            ProductId = 30,
            Quantity = 10,
            CreatedAt = now.AddDays(-1),
            CurrentStatus = ProductionStatus.Completed,
            CurrentStage = ProductionStage.Packaging,
            CompletedAt = now.AddDays(-1)
        };

        _context.ProductionOrders.AddRange(oldOrder, recentOrder);
        await _context.SaveChangesAsync();

        // Use a clean service instance with a fresh context to avoid caching issues
        var result = await _service.GetCompleteDashboardAsync();

        // Assert
        // Threshold is 60 days. Product 10 (no orders) and 20 (old order) should be stalled.
        result.StalledStock.Should().HaveCount(2);
        var skus = result.StalledStock.Select(s => s.Sku).ToList();
        skus.Should().Contain("SKU-A");
        skus.Should().Contain("SKU-B");
        
        // SKU-B should have around 100 days (or 999 if logic considers it 'never' due to some filter)
        result.StalledStock.First(s => s.Sku == "SKU-B").DaysSinceLastProduction.Should().BeInRange(60, 1000);
    }

    [Fact]
    public async Task GetCompleteDashboardAsync_ShouldCount_DelayedOrdersCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var delayedOrder = new ProductionOrder
        {
            LotCode = "DELAYED",
            ProductId = 1,
            Quantity = 10,
            CurrentStatus = ProductionStatus.InProduction,
            CurrentStage = ProductionStage.Sewing,
            EstimatedCompletionAt = now.AddDays(-1)
        };
        var onTimeOrder = new ProductionOrder
        {
            LotCode = "ONTIME",
            ProductId = 1,
            Quantity = 10,
            CurrentStatus = ProductionStatus.InProduction,
            CurrentStage = ProductionStage.Sewing,
            EstimatedCompletionAt = now.AddDays(1)
        };

        _context.ProductionOrders.AddRange(delayedOrder, onTimeOrder);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetCompleteDashboardAsync();

        // Assert
        result.DelayedOrdersCount.Should().Be(1);
    }

    [Fact]
    public async Task GetCompleteDashboardAsync_ShouldIdentify_TopProfitableModels()
    {
        // Arrange
        var productA = new Product { Id = 1, Name = "Model A", MainSku = "SKU-A", InternalCode = "C-A", FabricType = "F" };
        var productB = new Product { Id = 2, Name = "Model B", MainSku = "SKU-B", InternalCode = "C-B", FabricType = "F" };
        _context.Products.AddRange(productA, productB);

        var orderA = new ProductionOrder
        {
            LotCode = "OA",
            ProductId = 1,
            Quantity = 10,
            CurrentStatus = ProductionStatus.Completed,
            CurrentStage = ProductionStage.Packaging,
            AverageCostPerPiece = 10,
            ProfitMargin = 40,
            CompletedAt = DateTime.UtcNow
        };
        var orderB = new ProductionOrder
        {
            LotCode = "OB",
            ProductId = 2,
            Quantity = 10,
            CurrentStatus = ProductionStatus.Completed,
            CurrentStage = ProductionStage.Packaging,
            AverageCostPerPiece = 10,
            ProfitMargin = 20,
            CompletedAt = DateTime.UtcNow
        };

        _context.ProductionOrders.AddRange(orderA, orderB);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetCompleteDashboardAsync();

        // Assert
        result.TopProfitableModels.Should().NotBeEmpty();
        result.TopProfitableModels.First().Sku.Should().Be("SKU-A");
    }

    [Fact]
    public async Task GetCompleteDashboardAsync_ShouldHandleEmptyDatabase_Gracefully()
    {
        // Act
        var result = await _service.GetCompleteDashboardAsync();

        // Assert
        result.Should().NotBeNull();
        result.MonthProductionQuantity.Should().Be(0);
        result.MonthAverageCostPerPiece.Should().Be(0);
        result.MonthAverageMargin.Should().Be(0);
        result.DelayedOrdersCount.Should().Be(0);
        result.WeeklyVolumeData.Should().AllBeEquivalentTo(0);
    }

    [Fact]
    public async Task GetCompleteDashboardAsync_ShouldGenerate_7DaysChartDataCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var product = new Product { Id = 1, Name = "P", MainSku = "S", InternalCode = "C", FabricType = "F" };
        _context.Products.Add(product);

        var order1 = new ProductionOrder
        {
            LotCode = "O1",
            ProductId = 1,
            Quantity = 5,
            CurrentStatus = ProductionStatus.Completed,
            CurrentStage = ProductionStage.Packaging,
            CompletedAt = now.AddDays(-2)
        };
        var order2 = new ProductionOrder
        {
            LotCode = "O2",
            ProductId = 1,
            Quantity = 10,
            CurrentStatus = ProductionStatus.Completed,
            CurrentStage = ProductionStage.Packaging,
            CompletedAt = now
        };

        _context.ProductionOrders.AddRange(order1, order2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetCompleteDashboardAsync();

        // Assert
        result.WeeklyVolumeData.Should().HaveCount(7);
        result.WeeklyVolumeData.Last().Should().Be(10);
        result.WeeklyVolumeData[4].Should().Be(5);
    }
}
