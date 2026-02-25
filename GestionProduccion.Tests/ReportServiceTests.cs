using FluentAssertions;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Services.ProductionOrders;
using Moq;
using Xunit;

namespace GestionProduccion.Tests;

public class ReportServiceTests
{
    private readonly Mock<IProductionOrderQueryService> _mockQuery;
    private readonly Mock<ISystemConfigurationService> _mockConfig;
    private readonly ReportService _service;

    public ReportServiceTests()
    {
        _mockQuery = new Mock<IProductionOrderQueryService>();
        _mockConfig = new Mock<ISystemConfigurationService>();
        _service = new ReportService(_mockQuery.Object, _mockConfig.Object);
    }

    [Fact]
    public async Task GenerateProductionOrderReportAsync_ShouldReturnPdfBytes_WhenOrderExists()
    {
        // Arrange
        var orderId = 1;
        var orderDto = new ProductionOrderDto 
        { 
            Id = orderId, 
            LotCode = "OP-TEST", 
            ProductName = "Camisa Teste",
            CurrentStatus = "Completed",
            TotalCost = 500,
            AverageCostPerPiece = 10
        };

        _mockQuery.Setup(s => s.GetProductionOrderByIdAsync(orderId, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(orderDto);
        
        _mockQuery.Setup(s => s.GetHistoryByProductionOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new List<ProductionHistoryDto>());

        _mockConfig.Setup(s => s.GetConfigurationAsync())
                   .ReturnsAsync(new SystemConfigurationDto { CompanyName = "Test Corp" });

        // Act
        var result = await _service.GenerateProductionOrderReportAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        // Check for PDF signature (starts with %PDF)
        result[0].Should().Be(0x25); // %
        result[1].Should().Be(0x50); // P
        result[2].Should().Be(0x44); // D
        result[3].Should().Be(0x46); // F
    }

    [Fact]
    public async Task GenerateProductionOrderReportAsync_ShouldHandleNullLogo_Gracefully()
    {
        // Arrange
        var orderId = 1;
        _mockQuery.Setup(s => s.GetProductionOrderByIdAsync(orderId, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new ProductionOrderDto { LotCode = "OP-NO-LOGO" });
        _mockQuery.Setup(s => s.GetHistoryByProductionOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new List<ProductionHistoryDto>());

        _mockConfig.Setup(s => s.GetConfigurationAsync())
                   .ReturnsAsync(new SystemConfigurationDto { LogoBase64 = null }); // No Logo

        // Act
        // This should NOT throw an exception
        var result = await _service.GenerateProductionOrderReportAsync(orderId);

        // Assert
        result.Should().NotBeEmpty();
    }
}
