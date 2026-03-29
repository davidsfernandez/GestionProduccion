using Xunit;
using GestionProduccion.Data;
using Microsoft.EntityFrameworkCore;
using GestionProduccion.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GestionProduccion.Tests.Integration;

public class CRMSchemaIntegrityTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public CRMSchemaIntegrityTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CRM_Tables_ShouldBeAccessible_AfterInitialization()
    {
        // This test ensures that the CRM tables (Leads, Quotes, CustomerProfiles)
        // are physically present and accessible through the DbContext after
        // the application's automatic repair and migration logic has run.
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var exception = await Record.ExceptionAsync(async () =>
        {
            // Verify Leads table accessibility
            await context.Leads.FirstOrDefaultAsync();
            
            // Verify Quotes table accessibility
            await context.Quotes.FirstOrDefaultAsync();
            
            // Verify CustomerProfiles table accessibility
            await context.CustomerProfiles.FirstOrDefaultAsync();
        });

        // If the repair logic in Program.cs works, these queries should not 
        // throw "Table doesn't exist" exceptions.
        Assert.Null(exception);
    }
}
