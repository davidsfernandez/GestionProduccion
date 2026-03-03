using FluentAssertions;
using GestionProduccion.Data.Repositories;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Services;
using Moq;
using Xunit;

namespace GestionProduccion.Tests;

public class FinancialCalculatorServiceTests
{
    private readonly Mock<ISystemConfigurationRepository> _mockConfigRepo;
    private readonly Mock<IProductRepository> _mockProductRepo;
    private readonly FinancialCalculatorService _service;

    public FinancialCalculatorServiceTests()
    {
        _mockConfigRepo = new Mock<ISystemConfigurationRepository>();
        _mockProductRepo = new Mock<IProductRepository>();
        _service = new FinancialCalculatorService(_mockConfigRepo.Object, _mockProductRepo.Object);
    }

    [Fact]
    public async Task CalculateFinalOrderCostAsync_ShouldCalculateCorrectly_WhenDataIsValid()
    {
        // Arrange
        // Scenario: 2 hours duration, $50/hour, 10 items produced.
        // Expected Total Cost: $100
        // Expected Unit Cost: $10
        var order = new ProductionOrder
        {
            Quantity = 10,
            CreatedAt = DateTime.UtcNow.AddHours(-5),
            StartedAt = DateTime.UtcNow.AddHours(-2),
            CompletedAt = DateTime.UtcNow,
            ProductId = 1
        };

        var config = new SystemConfiguration { OperationalHourlyCost = 50m };
        _mockConfigRepo.Setup(r => r.GetAsync()).ReturnsAsync(config);

        var product = new Product { Id = 1, EstimatedSalePrice = 20m }; // Sale Price $20
        _mockProductRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        // Act
        await _service.CalculateFinalOrderCostAsync(order);

        // Assert
        order.TotalCost.Should().Be(100m); // 2h * 50
        order.AverageCostPerPiece.Should().Be(10m); // 100 / 10
        order.ProfitMargin.Should().Be(50m); // ((20 - 10) / 20) * 100 = 50%
    }

    [Fact]
    public async Task CalculateFinalOrderCostAsync_ShouldHandleZeroQuantity_ToAvoidDivisionByZero()
    {
        // Arrange
        var order = new ProductionOrder
        {
            Quantity = 0, // Edge case
            StartedAt = DateTime.UtcNow.AddHours(-1),
            CompletedAt = DateTime.UtcNow,
            ProductId = 1
        };

        var config = new SystemConfiguration { OperationalHourlyCost = 100m };
        _mockConfigRepo.Setup(r => r.GetAsync()).ReturnsAsync(config);
        _mockProductRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product)null!);

        // Act
        await _service.CalculateFinalOrderCostAsync(order);

        // Assert
        // Logic treats Quantity 0 as 1 to avoid crash
        order.TotalCost.Should().Be(100m);
        order.AverageCostPerPiece.Should().Be(100m);
    }

    [Fact]
    public async Task CalculateFinalOrderCostAsync_ShouldUseCreatedAt_WhenStartedAtIsNull()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var order = new ProductionOrder
        {
            Quantity = 1,
            CreatedAt = now.AddHours(-5),
            StartedAt = null, // Was never "Started" formally
            CompletedAt = now,
            ProductId = 1
        };

        var config = new SystemConfiguration { OperationalHourlyCost = 10m };
        _mockConfigRepo.Setup(r => r.GetAsync()).ReturnsAsync(config);

        // Act
        await _service.CalculateFinalOrderCostAsync(order);

        // Assert
        // Should calculate 5 hours duration from CreatedAt
        order.TotalCost.Should().Be(50m); // 5h * 10
    }
}
