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

public class UsersPageTests : TestContext
{
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;

    public UsersPageTests()
    {
        this.AddTestAuthorization().SetAuthorized("Admin").SetRoles("Administrator");
        JSInterop.Mode = JSRuntimeMode.Loose;

        _mockHttpHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_mockHttpHandler.Object) { BaseAddress = new Uri("http://localhost/") };
        Services.AddSingleton(httpClient);
        Services.AddSingleton(new ToastService());

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
        Services.AddSingleton(jsonOptions);
    }

    [Fact]
    public void UsersPage_ShouldShowValidationErrors_WhenEmailIsInvalid()
    {
        // Arrange
        SetupMockJsonResponse("api/Users", new List<UserDto>());
        SetupMockJsonResponse("api/SewingTeams", new ApiResponse<List<SewingTeamDto>> { Success = true, Data = new List<SewingTeamDto>() });

        var cut = RenderComponent<UsersPage>();

        // Act
        cut.Find("button.btn-primary").Click(); // Open Modal
        cut.Find("input[placeholder='email@exemplo.com']").Change("invalid-email");
        cut.Find("button[type=submit]").Click(); // Submit

        // Assert
        cut.WaitForState(() => cut.FindAll(".text-danger").Count > 0);
        var errorMessages = cut.FindAll(".text-danger").Select(e => e.TextContent);
        errorMessages.Should().ContainMatch("*Email*", "Should show email validation error");

        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public void UsersPage_ShouldUpdateTable_WhenUserSavedSuccessfully()
    {
        // Arrange
        SetupMockJsonResponse("api/Users", new List<UserDto>());
        SetupMockJsonResponse("api/SewingTeams", new ApiResponse<List<SewingTeamDto>> { Success = true, Data = new List<SewingTeamDto>() });

        // Mock POST success
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post && r.RequestUri!.ToString().EndsWith("api/Users")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        // Mock RE-FETCH after save
        var newUser = new UserDto { FullName = "New User", Email = "new@test.com", Role = GestionProduccion.Domain.Enums.UserRole.Operational };
        var updatedList = new List<UserDto> { newUser };
        var jsonList = JsonSerializer.Serialize(updatedList, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { new JsonStringEnumConverter() } });

        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri!.ToString().EndsWith("api/Users")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonList) });

        var cut = RenderComponent<UsersPage>();

        // Act
        cut.Find("button.btn-primary").Click(); // Open
        cut.Find("input[placeholder='Nome completo']").Change("New User");
        cut.Find("input[placeholder='email@exemplo.com']").Change("new@test.com");
        // Select Role (assuming select exists)
        // cut.Find("select").Change("Operational"); 
        cut.Find("button[type=submit]").Click(); // Save

        // Assert
        // Wait for table row
        cut.WaitForState(() => cut.FindAll("tbody tr").Count > 0);
        cut.Markup.Should().Contain("New User");
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
