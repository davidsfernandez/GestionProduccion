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

public class CatalogPageTests : TestContext
{
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;

    public CatalogPageTests()
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
    public void CatalogPage_ShouldHandleDecimalInput_WithPtBRCulture()
    {
        // Set culture
        var culture = new CultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        // Arrange
        SetupMockJsonResponse("api/Products", new ApiResponse<List<ProductDto>> { Success = true, Data = new List<ProductDto>() });

        var cut = RenderComponent<CatalogPage>();

        // Act
        // Open modal
        cut.Find("button.btn-primary").Click();

        // Find price input (EstimatedSalePrice)
        // Blazor input number handles culture. "10,50" -> 10.50
        var input = cut.Find("input[type=number]"); // Or step="0.01"
        input.Change("10,50");

        // Assert
        // We verify that no validation error occurs for "The field EstimatedSalePrice must be a number."
        // And internal model is updated.
        // We can check validation message absence.
        cut.FindAll(".validation-message").Should().BeEmpty();
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
