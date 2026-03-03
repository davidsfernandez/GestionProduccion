using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Tests.Integration;

public class SettingsIntegrationTests : BaseIntegrationTest
{
    public SettingsIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task UploadLogo_ShouldHandleLargeFiles_OrReturnError()
    {
        // Arrange
        await SeedDataAsync();
        AuthenticateAs(UserRole.Administrator);

        var content = new MultipartFormDataContent();
        var fileBytes = new byte[1024 * 512]; // 512KB ficticio
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "logo.png");

        // Act
        var response = await Client.PostAsync("/api/Users/upload-avatar", content);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.UnsupportedMediaType);
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Cors_ShouldAllowAnyOrigin()
    {
        // Act
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/Configuration/logo");
        request.Headers.Add("Origin", "http://any-domain.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await Client.SendAsync(request);

        // Assert
        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeTrue();
    }
}
