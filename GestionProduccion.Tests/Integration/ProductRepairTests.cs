using Xunit;
using GestionProduccion.Data;
using Microsoft.EntityFrameworkCore;
using GestionProduccion.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace GestionProduccion.Tests.Integration;

public class ProductRepairTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public ProductRepairTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ProductsTable_ShouldHaveVisualColumns_AfterInitialization()
    {
        // This test ensures that the Products table schema includes the necessary 
        // visual fields required for catalog display and bonus calculations.
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var exception = await Record.ExceptionAsync(async () =>
        {
            // Perform a query that selects the visual fields to ensure they are 
            // properly mapped and exist in the schema.
            var product = await context.Products
                .Select(p => new { p.AvailableColors, p.AvailableSizes, p.ImageUrl, p.Description })
                .FirstOrDefaultAsync();
        });

        // The expectation is that the initialization sequence (Migrations + Repairs)
        // ensures these columns exist, preventing runtime crashes.
        Assert.Null(exception);
    }
}
