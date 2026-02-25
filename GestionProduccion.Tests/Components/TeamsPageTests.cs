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

public class TeamsPageTests : TestContext
{
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;

    public TeamsPageTests()
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
    public void TeamsPage_ShouldDisableCreateButton_WhenNoEligibleUsers()
    {
        // Arrange
        // Return 0 eligible users
        SetupMockJsonResponse("api/Users", new List<UserDto>());
        SetupMockJsonResponse("api/SewingTeams", new ApiResponse<List<SewingTeamDto>> { Success = true, Data = new List<SewingTeamDto>() });

        // Act
        var cut = RenderComponent<TeamsPage>();

        // Assert
        cut.WaitForState(() => cut.FindAll("button").Count > 0);

        // Button "Adicionar Equipe" should be disabled
        var btn = cut.Find("button.btn-primary");
        btn.HasAttribute("disabled").Should().BeTrue("Create button should be disabled if no users exist");

        cut.Markup.Should().Contain("Não é possível criar equipes", "Warning message should be displayed");
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
