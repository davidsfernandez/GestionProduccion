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
        // Setup Auth
        this.AddTestAuthorization().SetAuthorized("Test User").SetRoles("Operational");

        // Setup JS Interop
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Setup HttpClient Mock
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpHandler.Object) { BaseAddress = new Uri("http://localhost/") };
        Services.AddSingleton(_httpClient);

        // Setup JSON Options
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
        Services.AddSingleton(jsonOptions);

        // Setup Services
        _mockLifecycleClient = new Mock<IProductionOrderLifecycleClient>();
        Services.AddSingleton(_mockLifecycleClient.Object);

        // ToastService is registered as Singleton in Client Program.cs. 
        // If it's a concrete class without virtual methods, we might need to use the real one or extract interface.
        // Assuming it's simple enough to use or mock if methods are virtual.
        // Let's register a mock if possible, otherwise we might need to register the real one if it has no external dependencies.
        // For now, let's try to mock it assuming we can or use a dummy.
        // Actually, ToastService in Client seems to be a concrete class. 
        // Best practice is to extract interface, but for this test I will register a real instance or a loose mock if virtual.
        // I will add a dummy ToastService for now.
        Services.AddSingleton(new ToastService());
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
        // Check for tabs
        cut.FindAll(".nav-link").Should().HaveCount(2);
        
        // Initial tab is 'production', check if order is rendered
        cut.WaitForState(() => cut.FindAll(".badge.bg-info-soft").Count > 0);
        cut.Find(".badge.bg-info-soft").TextContent.Should().Contain("Equipe");
        cut.Find(".fw-bold.text-dark").TextContent.Should().Contain("Shirt");

        // Switch tab to admin
        cut.FindAll(".nav-link")[1].Click();
        cut.WaitForState(() => cut.FindAll(".card.border-warning").Count > 0);
        cut.Find("h5.card-title").TextContent.Should().Contain("Buy Ink");
    }

    private void SetupMockJsonResponse<T>(string url, T response)
    {
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { new JsonStringEnumConverter() } });
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().EndsWith(url) && r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });
    }
}
