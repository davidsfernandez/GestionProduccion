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
using GestionProduccion.Models.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace GestionProduccion.Tests.Components;

public class DashboardTests : TestContext
{
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<ISignalRService> _mockSignalR;

    public DashboardTests()
    {
        this.AddTestAuthorization().SetAuthorized("Admin").SetRoles("Administrator");
        JSInterop.Mode = JSRuntimeMode.Loose;

        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpHandler.Object) { BaseAddress = new Uri("http://localhost/") };
        Services.AddSingleton(_httpClient);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
        Services.AddSingleton(jsonOptions);

        _mockSignalR = new Mock<ISignalRService>();
        Services.AddSingleton(_mockSignalR.Object);

        var audioService = new AudioService(JSInterop.JSRuntime);
        Services.AddSingleton(audioService);
        Services.AddSingleton(new ToastService(audioService));

        Services.AddSingleton(new UserStateService());
    }

    [Fact(Skip = "Unstable in headless CI environment due to polling and JS interop")]
    public void Dashboard_ShouldRender_FinancialMetrics_InBrazilianFormat()
    {
        var culture = new CultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        var dashboardDto = new DashboardCompleteResponse
        {
            MonthAverageCostPerPiece = 15.5m,
            MonthAverageMargin = 45.2m,
            MonthProductionQuantity = 1000,
            DelayedOrdersCount = 3,
            StalledStock = new List<StalledProductDto>()
        };

        var json = JsonSerializer.Serialize(ApiResponse<DashboardCompleteResponse>.SuccessResult(dashboardDto), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("api/Dashboard/complete") && r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(json) });

        // Mock Tasks endpoint to avoid noise
        var tasksJson = JsonSerializer.Serialize(ApiResponse<List<TaskDto>>.SuccessResult(new List<TaskDto>()), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("api/Tasks") && r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(tasksJson) });

        var cut = RenderComponent<Home>();

        cut.WaitForState(() => cut.FindAll(".card-body h3").Count > 0, TimeSpan.FromSeconds(5));

        var cards = cut.FindAll(".card-body h3");
        cards[0].TextContent.Should().Contain("15,50");
        cards[1].TextContent.Should().Contain("45,2");
    }

    [Fact(Skip = "Unstable in headless CI environment")]
    public void Dashboard_ShouldRender_DeadStockAndDelayedOrders_Counters()
    {
        var dashboardDto = new DashboardCompleteResponse
        {
            DelayedOrdersCount = 3,
            StalledStock = new List<StalledProductDto> { new(), new() }
        };

        var json = JsonSerializer.Serialize(ApiResponse<DashboardCompleteResponse>.SuccessResult(dashboardDto), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("api/Dashboard/complete") && r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(json) });

        // Mock Tasks endpoint to avoid noise
        var tasksJson = JsonSerializer.Serialize(ApiResponse<List<TaskDto>>.SuccessResult(new List<TaskDto>()), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("api/Tasks") && r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(tasksJson) });

        var cut = RenderComponent<Home>();

        cut.WaitForState(() => cut.FindAll(".card-body h3").Count > 0, TimeSpan.FromSeconds(5));
        var cards = cut.FindAll(".card-body h3");
        cards[3].TextContent.Should().Contain("3");
    }

    [Fact]
    public void Dashboard_ShouldInvokeJS_ToRenderChart()
    {
        // Arrange
        var dashboardDto = new DashboardDto { TotalActiveOrders = 5 };

        var json = JsonSerializer.Serialize(new ApiResponse<DashboardDto> { Success = true, Data = dashboardDto }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { new JsonStringEnumConverter() } });
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("api/Dashboard") && !r.RequestUri!.ToString().Contains("completo") && r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(json) });

        // Setup the expected JS calls in Home.razor
        JSInterop.SetupVoid("seronaCharts.renderRevenueChart", _ => true);
        JSInterop.SetupVoid("seronaCharts.renderBarChart", _ => true);

        // Act
        RenderComponent<Home>();

        // Assert
        JSInterop.VerifyInvoke("seronaCharts.renderRevenueChart", 1);
    }
}


