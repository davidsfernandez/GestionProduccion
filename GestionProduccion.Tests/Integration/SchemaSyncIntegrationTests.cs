using Xunit;
using GestionProduccion.Data;
using Microsoft.EntityFrameworkCore;
using GestionProduccion.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GestionProduccion.Tests.Integration;

public class SchemaSyncIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public SchemaSyncIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Seeding_ShouldSucceed_WhenSchemaIsSynchronized()
    {
        // This test ensures that the database seeding logic can execute 
        // without crashing, confirming that any required schema columns 
        // are correctly verified or handled during initialization.
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        var exception = await Record.ExceptionAsync(async () => 
        {
            await DbInitializer.SeedAsync(context, logger);
        });

        Assert.Null(exception);
    }
}
