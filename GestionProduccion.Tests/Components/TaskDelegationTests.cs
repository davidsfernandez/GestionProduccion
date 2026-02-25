using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using GestionProduccion.Client.Pages;
using GestionProduccion.Client.Services;
using GestionProduccion.Models.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace GestionProduccion.Tests.Components;

public class TaskDelegationTests : TestContext
{
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly HttpClient _httpClient;

    public TaskDelegationTests()
    {
        this.AddTestAuthorization().SetAuthorized("Manager").SetRoles("Administrator");
        JSInterop.Mode = JSRuntimeMode.Loose;

        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpHandler.Object) { BaseAddress = new Uri("http://localhost/") };
        Services.AddSingleton(_httpClient);
        Services.AddSingleton(new ToastService());
        
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
        Services.AddSingleton(jsonOptions);
    }

    [Fact]
    public void DelegationForm_ShouldShowValidationErrors_WhenFieldsAreEmpty()
    {
        // Arrange
        // Mock initial load calls to avoid errors
        SetupMockJsonResponse("api/Users", new List<UserDto>());
        SetupMockJsonResponse("api/Tasks", new ApiResponse<List<TaskDto>> { Success = true, Data = new List<TaskDto>() });

        var cut = RenderComponent<TaskDelegationPage>();

        // Act
        // Initial state is empty. Click submit directly.
        cut.Find("button[type=submit]").Click();

        // Assert
        // Validation messages usually have class 'validation-message' or are inside 'li.validation-message'
        cut.WaitForState(() => cut.FindAll(".text-danger").Count > 0);
        var errors = cut.FindAll(".text-danger");
        errors.Should().NotBeEmpty();
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
