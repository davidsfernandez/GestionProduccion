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

        Services.AddSingleton(new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        _mockSignalR = new Mock<ISignalRService>();
        Services.AddSingleton(_mockSignalR.Object);
    }

    [Fact]
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

        var json = JsonSerializer.Serialize(dashboardDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().EndsWith("api/Dashboard/completo") && r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(json) });

        var cut = RenderComponent<Home>();

        cut.WaitForState(() => cut.FindAll(".card-body h3").Count > 0);

        var cards = cut.FindAll(".card-body h3");
        cards[0].TextContent.Should().Contain("15,50");
        cards[1].TextContent.Should().Contain("45,2");
    }

    [Fact]
    public void Dashboard_ShouldRender_DeadStockAndDelayedOrders_Counters()
    {
        var dashboardDto = new DashboardCompleteResponse
        {
            DelayedOrdersCount = 3,
            StalledStock = new List<StalledProductDto> { new(), new() }
        };

        var json = JsonSerializer.Serialize(dashboardDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().EndsWith("api/Dashboard/completo") && r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(json) });

        var cut = RenderComponent<Home>();

        cut.WaitForState(() => cut.FindAll(".card-body h3").Count > 0);
        var cards = cut.FindAll(".card-body h3");
        cards[3].TextContent.Should().Contain("3");
    }

    [Fact]
    public void Dashboard_ShouldInvokeJS_ToRenderChart()
    {
        // Arrange
        var dashboardDto = new DashboardCompleteResponse
        {
            WeeklyLabels = new List<string> { "Seg", "Ter", "Qua", "Qui", "Sex", "Sab", "Dom" },
            WeeklyVolumeData = new List<int> { 10, 20, 15, 30, 25, 40, 35 },
            // Populate other fields to avoid null refs if any
            StalledStock = new List<StalledProductDto>(),
            ProductionByWorkshop = new List<WorkshopProductionDto>(),
            TopProfitableModels = new List<ProductProfitabilityDto>(),
            BottomProfitableModels = new List<ProductProfitabilityDto>()
        };

        var json = JsonSerializer.Serialize(dashboardDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().EndsWith("api/Dashboard/completo") && r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(json) });

        // Setup the expected JS call
        JSInterop.SetupVoid("seronaCharts.renderDashboardChart", _ => true);

        // Act
        RenderComponent<Home>();

        // Assert
        JSInterop.VerifyInvoke("seronaCharts.renderDashboardChart", 1);
    }
}
