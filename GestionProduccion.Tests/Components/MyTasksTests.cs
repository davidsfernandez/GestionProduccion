using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using GestionProduccion.Client.Pages;
using GestionProduccion.Client.Services;
using GestionProduccion.Client.Services.ProductionOrders;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace GestionProduccion.Tests.Components;

public class MyTasksTests : TestContext
{
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<IProductionOrderLifecycleClient> _mockLifecycleClient;

    public MyTasksTests()
    {
        this.AddTestAuthorization().SetAuthorized("Test User").SetRoles("Operational");
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

        _mockLifecycleClient = new Mock<IProductionOrderLifecycleClient>();
        Services.AddSingleton(_mockLifecycleClient.Object);

        var audioService = new AudioService(JSInterop.JSRuntime);
        Services.AddSingleton(audioService);
        Services.AddSingleton(new ToastService(audioService));
    }

    [Fact]
    public void MyTasks_ShouldRender_BothProductionAndAdminTasks()
    {
        // Arrange
        var orders = new List<ProductionOrderDto>
        {
            new() { Id = 1, LotCode = "OP-1", ProductName = "Shirt", IsTeamTask = true, CurrentStatus = "Pending", EstimatedCompletionAt = DateTime.Now.AddDays(1) }
        };
        var adminTasks = new List<TaskDto>
        {
            new() { Id = 10, Title = "Buy Ink", Status = "Pending", Deadline = DateTime.Now.AddDays(1) }
        };

        SetupMockJsonResponse("api/Tasks/my", new ApiResponse<List<ProductionOrderDto>> { Success = true, Data = orders });
        SetupMockJsonResponse("api/Tasks/my-admin", new ApiResponse<List<TaskDto>> { Success = true, Data = adminTasks });

        // Act
        var cut = RenderComponent<MyTasks>();

        // Assert
        cut.FindAll(".nav-link").Should().HaveCount(2);

        // Check production task
        cut.WaitForState(() => cut.FindAll(".badge.bg-info-soft").Count > 0);
        cut.Find(".badge.bg-info-soft").TextContent.Should().Contain("Equipe");

        // Check admin task logic (switch tab)
        cut.FindAll(".nav-link")[1].Click();
        cut.WaitForState(() => cut.FindAll(".card.border-warning").Count > 0);
        cut.Find("h5.card-title").TextContent.Should().Contain("Buy Ink");
    }

    [Fact]
    public void MyTasks_AdminTaskProgressBar_ShouldReflectProgress()
    {
        // Arrange
        var adminTasks = new List<TaskDto>
        {
            new() { Id = 10, Title = "Urgent", Status = "Pending", Deadline = DateTime.Now.AddDays(1), ProgressPercentage = 75 }
        };

        SetupMockJsonResponse("api/Tasks/my", new ApiResponse<List<ProductionOrderDto>> { Success = true, Data = new List<ProductionOrderDto>() });
        SetupMockJsonResponse("api/Tasks/my-admin", new ApiResponse<List<TaskDto>> { Success = true, Data = adminTasks });

        // Act
        var cut = RenderComponent<MyTasks>();

        // Switch to admin tab
        cut.FindAll(".nav-link")[1].Click();
        cut.WaitForState(() => cut.FindAll(".progress-bar").Count > 0);

        // Assert
        var progressBar = cut.Find(".progress-bar");
        progressBar.GetAttribute("style").Should().Contain("width: 75%");
    }

    private void SetupMockJsonResponse<T>(string url, T response)
    {
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { new JsonStringEnumConverter() } });
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().EndsWith(url) && r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(json) });
    }
}
