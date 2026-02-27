using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using GestionProduccion.Client.Auth;
using GestionProduccion.Client.Pages;
using GestionProduccion.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Xunit;

namespace GestionProduccion.Tests.Components;

public class LoginPageTests : TestContext
{
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;

    public LoginPageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;

        _mockHttpHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_mockHttpHandler.Object) { BaseAddress = new Uri("http://localhost/") };
        Services.AddSingleton(httpClient);

        var audioService = new AudioService(JSInterop.JSRuntime);
        Services.AddSingleton(audioService);
        Services.AddSingleton(new ToastService(audioService));

        Services.AddSingleton(new UserStateService());

        this.AddTestAuthorization().SetNotAuthorized();
    }

    [Fact]
    public void Login_ShouldShowValidationErrors_WhenFieldsAreEmpty()
    {
        // Arrange
        // Mock checking setup required if the component does it on init
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("false") });

        var cut = RenderComponent<LoginPage>();

        // Act
        // Find submit button and click without filling anything
        var submitButton = cut.Find("button[type=submit]");
        submitButton.Click();

        // Assert
        // Check for validation messages (ValidationSummary or ValidationMessage)
        // Usually these render inside elements with class 'validation-message' or 'text-danger'
        var validationErrors = cut.FindAll(".validation-message");
        if (validationErrors.Count == 0)
        {
            validationErrors = cut.FindAll(".text-danger");
        }

        validationErrors.Should().NotBeEmpty("Validation messages should be visible when form is empty");

        // Verify POST (Login) was NOT called
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post && r.RequestUri!.ToString().Contains("login")),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}
