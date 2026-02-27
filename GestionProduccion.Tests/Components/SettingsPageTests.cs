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

public class SettingsPageTests : TestContext
{
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;

    public SettingsPageTests()
    {
        this.AddTestAuthorization().SetAuthorized("Admin").SetRoles("Administrator");
        JSInterop.Mode = JSRuntimeMode.Loose;

        _mockHttpHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_mockHttpHandler.Object) { BaseAddress = new Uri("http://localhost/") };
        Services.AddSingleton(httpClient);

        var audioService = new AudioService(JSInterop.JSRuntime);
        Services.AddSingleton(audioService);
        Services.AddSingleton(new ToastService(audioService));

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
        Services.AddSingleton(jsonOptions);
    }

    [Fact]
    public void SettingsPage_ShouldConvertUploadedFile_ToBase64()
    {
        // Arrange
        var config = new SystemConfigurationDto { CompanyName = "Test Co", OperationalHourlyCost = 10 };
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(json) });

        var cut = RenderComponent<SettingsPage>();
        cut.WaitForState(() => cut.FindAll("input[type=file]").Count > 0);

        // Act
        var inputFile = cut.FindComponent<Microsoft.AspNetCore.Components.Forms.InputFile>();
        var fileContent = new byte[] { 1, 2, 3 };
        var file = InputFileContent.CreateFromBinary(fileContent, "logo.png", contentType: "image/png");

        // Simulate upload
        inputFile.UploadFiles(file);

        // Assert
        // Verify internal state or visual feedback (img src)
        // Since we can't easily access private state, checking if the image preview appears or changes
        // Assuming there is an <img> tag that shows the logo
        // Or verify the model update if we could inspect it. 
        // Best approach: Check if an <img> tag has a data URI.

        cut.WaitForState(() => cut.FindAll("img").Count > 0);
        var img = cut.Find("img");
        img.GetAttribute("src").Should().Contain("data:image/png;base64,");
    }
}
