/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using GestionProduccion.Client.Pages;
using GestionProduccion.Client.Services;
using GestionProduccion.Client.Services.ProductionOrders;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Resources;
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

        var audioService = new AudioService(JSInterop.JSRuntime);
        Services.AddSingleton(audioService);
        Services.AddSingleton(new ToastService(audioService));
        Services.AddScoped(_ => new Mock<ISignalRService>().Object);

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
            .ReturnsAsync(ApiResponse<ProductionOrderDto>.SuccessResult(order));
        _mockQueryClient.Setup(c => c.GetHistoryByProductionOrderIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<ProductionHistoryDto>>.SuccessResult(new List<ProductionHistoryDto>()));

        // Act
        var cut = RenderComponent<OrderDetails>(parameters => parameters.Add(p => p.Id, 1));

        // Assert
        cut.WaitForState(() => cut.FindAll("h5.card-title").Count > 0);
        // Look for the base word without accents if possible or use the constant directly
        // The issue is that the rendered markup might be different from the source constant 
        // due to encoding mismatches in the test runner.
        cut.Markup.Should().Contain("Financeira"); 
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
            .ReturnsAsync(ApiResponse<ProductionOrderDto>.SuccessResult(order));
        _mockQueryClient.Setup(c => c.GetHistoryByProductionOrderIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<ProductionHistoryDto>>.SuccessResult(new List<ProductionHistoryDto>()));

        // Act
        var cut = RenderComponent<OrderDetails>(parameters => parameters.Add(p => p.Id, 2));

        // Assert
        cut.WaitForState(() => cut.FindAll("h5.card-title").Count > 0);
        cut.Markup.Should().NotContain(Portuguese.OP_FinancialAnalysis);
    }

    [Fact]
    public void OrderDetails_ShouldShowStartedAndCompletedInputs_WhenEditModalOpens()
    {
        var order = new ProductionOrderDto
        {
            Id = 3,
            LotCode = "OP-EDIT-1",
            CurrentStatus = "Completed",
            TotalCost = 250m,
            AverageCostPerPiece = 12.5m,
            Quantity = 20,
            EstimatedCompletionAt = DateTime.Now,
            CreatedAt = DateTime.Now,
            StartedAt = DateTime.UtcNow.AddHours(-2),
            CompletedAt = DateTime.UtcNow.AddHours(-1)
        };

        _mockQueryClient.Setup(c => c.GetProductionOrderByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductionOrderDto>.SuccessResult(order));
        _mockQueryClient.Setup(c => c.GetHistoryByProductionOrderIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<ProductionHistoryDto>>.SuccessResult(new List<ProductionHistoryDto>()));

        var cut = RenderComponent<OrderDetails>(parameters => parameters.Add(p => p.Id, 3));
        cut.WaitForState(() => cut.FindAll("button").Any());

        cut.FindAll("button").First(b => b.TextContent.Contains("Editar") || b.TextContent.Contains("Edit")).Click();

        cut.Markup.Should().Contain("Data/hora real de início");
        cut.Markup.Should().Contain("Data/hora real de fim");
        cut.FindAll("input[type=\"datetime-local\"]").Count.Should().Be(2);
    }
}


