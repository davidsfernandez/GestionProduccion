using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Tests.Integration;

public class ReportsIntegrationTests : BaseIntegrationTest
{
    public ReportsIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetDailyReport_ShouldReturnValidPdf_WithApplicationPdfContentType()
    {
        // Arrange
        await SeedDataAsync();
        AuthenticateAs(UserRole.Administrator);

        // Act
        var response = await Client.GetAsync("/api/ProductionOrders/daily-report");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");

        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        pdfBytes.Length.Should().BeGreaterThan(0);

        // Magic number PDF: %PDF-
        var header = System.Text.Encoding.ASCII.GetString(pdfBytes.Take(5).ToArray());
        header.Should().Be("%PDF-");
    }

    [Fact]
    public async Task GetOrderPdf_ShouldReturnValidPdf_ForExistingOrder()
    {
        // Arrange
        await SeedDataAsync();
        AuthenticateAs(UserRole.Administrator);

        // 1. Obtener ID de una orden (del seed o crear una)
        var orderRequest = new CreateProductionOrderRequest
        {
            ProductId = 1,
            Quantity = 5,
            EstimatedCompletionAt = DateTime.Now.AddDays(5),
            Size = "G"
        };
        var createResp = await Client.PostAsJsonAsync("/api/ProductionOrders", orderRequest);
        var order = await createResp.Content.ReadFromJsonAsync<ProductionOrderDto>(JsonOptions);

        // Act
        var response = await Client.GetAsync($"/api/ProductionOrders/{order!.Id}/pdf");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue("PDF endpoint should return 200 OK");
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");

        var bytes = await response.Content.ReadAsByteArrayAsync();
        var header = System.Text.Encoding.ASCII.GetString(bytes.Take(5).ToArray());
        header.Should().Be("%PDF-", "Should be a valid PDF file");
    }
}
