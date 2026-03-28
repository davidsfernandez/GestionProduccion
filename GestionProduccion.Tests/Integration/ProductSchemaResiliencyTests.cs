using Xunit;
using GestionProduccion.Data;
using Microsoft.EntityFrameworkCore;
using GestionProduccion.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GestionProduccion.Tests.Integration;

public class ProductSchemaResiliencyTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public ProductSchemaResiliencyTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Seeding_ShouldGracefullyHandleMissingProductColumns()
    {
        // This test ensures that the database seeding logic, specifically 
        // the BackfillBonusesAsync operation, does not crash the application 
        // if certain visual columns are missing from the Products table.
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        var exception = await Record.ExceptionAsync(async () => 
        {
            await DbInitializer.SeedAsync(context, logger);
        });

        // The expectation is that the seeder completes successfully even if 
        // it has to skip certain operations due to schema mismatches.
        Assert.Null(exception);
    }
}
