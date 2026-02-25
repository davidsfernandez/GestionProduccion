using System.Net;
using FluentAssertions;
using GestionProduccion.Domain.Enums;

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

        // Magic number PDF: %PDF- (25 50 44 46 2D)
        var header = System.Text.Encoding.ASCII.GetString(pdfBytes.Take(5).ToArray());
        header.Should().Be("%PDF-");
    }
}
