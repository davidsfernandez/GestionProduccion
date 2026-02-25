using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using GestionProduccion.Client.Pages;
using GestionProduccion.Client.Services;
using GestionProduccion.Client.Services.ProductionOrders;
using GestionProduccion.Models.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace GestionProduccion.Tests.Components;

public class OrderDetailsTests : TestContext
{
    private readonly Mock<IProductionOrderQueryClient> _mockQueryClient;
    private readonly Mock<IProductionOrderLifecycleClient> _mockLifecycleClient;
    private readonly Mock<IProductionOrderMutationClient> _mockMutationClient;
    private readonly Mock<IProductClient> _mockProductClient;
    private readonly Mock<ISewingTeamClient> _mockTeamClient;

    public OrderDetailsTests()
    {
        this.AddTestAuthorization().SetAuthorized("User").SetRoles("Administrator");
        JSInterop.Mode = JSRuntimeMode.Loose;

        _mockQueryClient = new Mock<IProductionOrderQueryClient>();
        Services.AddSingleton(_mockQueryClient.Object);

        _mockLifecycleClient = new Mock<IProductionOrderLifecycleClient>();
        Services.AddSingleton(_mockLifecycleClient.Object);

        _mockMutationClient = new Mock<IProductionOrderMutationClient>();
        Services.AddSingleton(_mockMutationClient.Object);

        _mockProductClient = new Mock<IProductClient>();
        Services.AddSingleton(_mockProductClient.Object);

        _mockTeamClient = new Mock<ISewingTeamClient>();
        Services.AddSingleton(_mockTeamClient.Object);

        Services.AddSingleton(new ToastService());
        Services.AddSingleton(new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    [Fact]
    public void OrderDetails_ShouldShowRealCost_WhenStatusIsCompleted()
    {
        // Arrange
        var order = new ProductionOrderDto
        {
            Id = 1,
            LotCode = "OP-FIN-1",
            CurrentStatus = "Completed",
            TotalCost = 500m,
            AverageCostPerPiece = 25.50m, // Specific value for test
            Quantity = 50,
            EstimatedCompletionAt = DateTime.Now,
            CreatedAt = DateTime.Now
        };

        _mockQueryClient.Setup(c => c.GetProductionOrderByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _mockQueryClient.Setup(c => c.GetHistoryByProductionOrderIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductionHistoryDto>());

        // Act
        var cut = RenderComponent<OrderDetails>(parameters => parameters.Add(p => p.Id, 1));

        // Assert
        cut.WaitForState(() => cut.FindAll("h5.card-title").Count > 0);
        cut.Markup.Should().Contain("Análise Financeira");
        cut.Markup.Should().Contain("R$ 25,50"); // Specific check
    }

    [Fact]
    public void OrderDetails_ShouldHideRealCost_WhenStatusIsPending()
    {
        // Arrange
        var order = new ProductionOrderDto
        {
            Id = 2,
            LotCode = "OP-PEND-1",
            CurrentStatus = "Pending",
            TotalCost = 0,
            Quantity = 50,
            EstimatedCompletionAt = DateTime.Now,
            CreatedAt = DateTime.Now
        };

        _mockQueryClient.Setup(c => c.GetProductionOrderByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _mockQueryClient.Setup(c => c.GetHistoryByProductionOrderIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductionHistoryDto>());

        // Act
        var cut = RenderComponent<OrderDetails>(parameters => parameters.Add(p => p.Id, 2));

        // Assert
        cut.WaitForState(() => cut.FindAll("h5.card-title").Count > 0);
        cut.Markup.Should().NotContain("Análise Financeira");
    }
}
