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
        // Product A: No orders (Dead)
        var productA = new Product { Id = 1, Name = "Product A", MainSku = "SKU-A", InternalCode = "C-A", FabricType = "F" };
        // Product B: Order 65 days ago (Dead)
        var productB = new Product { Id = 2, Name = "Product B", MainSku = "SKU-B", InternalCode = "C-B", FabricType = "F" };
        // Product C: Order 5 days ago (Active)
        var productC = new Product { Id = 3, Name = "Product C", MainSku = "SKU-C", InternalCode = "C-C", FabricType = "F" };
        
        _context.Products.AddRange(productA, productB, productC);

        var now = DateTime.UtcNow;
        var oldOrder = new ProductionOrder 
        { 
            LotCode = "OLD", ProductId = 2, Quantity = 10, CreatedAt = now.AddDays(-65), 
            CurrentStatus = ProductionStatus.Completed, CurrentStage = ProductionStage.Packaging 
        };
        var recentOrder = new ProductionOrder 
        { 
            LotCode = "RECENT", ProductId = 3, Quantity = 10, CreatedAt = now.AddDays(-5), 
            CurrentStatus = ProductionStatus.Completed, CurrentStage = ProductionStage.Packaging 
        };

        _context.ProductionOrders.AddRange(oldOrder, recentOrder);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetCompleteDashboardAsync();

        // Assert
        // Logic: activeProductIds = orders where CreatedAt >= now.AddDays(-60)
        // activeProductIds should contain only ProductId 3
        // StalledStock = products not in activeProductIds (1 and 2)
        result.StalledStock.Should().HaveCount(2);
        result.StalledStock.Select(s => s.Sku).Should().Contain(new[] { "SKU-A", "SKU-B" });
    }

    [Fact]
    public async Task GetCompleteDashboardAsync_ShouldCount_DelayedOrdersCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var delayedOrder = new ProductionOrder 
        { 
            LotCode = "DELAYED", ProductId = 1, Quantity = 10, 
            CurrentStatus = ProductionStatus.InProduction, CurrentStage = ProductionStage.Sewing,
            EstimatedCompletionAt = now.AddDays(-1) // Yesterday
        };
        var onTimeOrder = new ProductionOrder 
        { 
            LotCode = "ONTIME", ProductId = 1, Quantity = 10, 
            CurrentStatus = ProductionStatus.InProduction, CurrentStage = ProductionStage.Sewing,
            EstimatedCompletionAt = now.AddDays(1) // Tomorrow
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

        // Model A: Margin 40%
        var orderA = new ProductionOrder 
        { 
            LotCode = "OA", ProductId = 1, Quantity = 10, 
            CurrentStatus = ProductionStatus.Completed, CurrentStage = ProductionStage.Packaging,
            AverageCostPerPiece = 10, ProfitMargin = 40
        };
        // Model B: Margin 20%
        var orderB = new ProductionOrder 
        { 
            LotCode = "OB", ProductId = 2, Quantity = 10, 
            CurrentStatus = ProductionStatus.Completed, CurrentStage = ProductionStage.Packaging,
            AverageCostPerPiece = 10, ProfitMargin = 20
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
            LotCode = "O1", ProductId = 1, Quantity = 5, 
            CurrentStatus = ProductionStatus.Completed, CurrentStage = ProductionStage.Packaging,
            CompletedAt = now.AddDays(-2) 
        };
        var order2 = new ProductionOrder 
        { 
            LotCode = "O2", ProductId = 1, Quantity = 3, 
            CurrentStatus = ProductionStatus.Completed, CurrentStage = ProductionStage.Packaging,
            CompletedAt = now 
        };
        var order3 = new ProductionOrder 
        { 
            LotCode = "O3", ProductId = 1, Quantity = 7, 
            CurrentStatus = ProductionStatus.Completed, CurrentStage = ProductionStage.Packaging,
            CompletedAt = now 
        };

        _context.ProductionOrders.AddRange(order1, order2, order3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetCompleteDashboardAsync();

        // Assert
        // WeeklyVolumeData has 7 items. Last item is today. 3rd to last is 2 days ago.
        // Today sum: 3 + 7 = 10
        // 2 days ago sum: 5
        result.WeeklyVolumeData.Should().HaveCount(7);
        result.WeeklyVolumeData.Last().Should().Be(10);
        result.WeeklyVolumeData[4].Should().Be(5);
        result.WeeklyVolumeData[0].Should().Be(0);
    }
}
