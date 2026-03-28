using Xunit;
using GestionProduccion.Data;
using Microsoft.EntityFrameworkCore;
using GestionProduccion.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace GestionProduccion.Tests.Integration;

public class DatabaseSchemaTests : BaseIntegrationTest
{
    public DatabaseSchemaTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task VerifySchema_ProductionOrderHasCustomerUserId()
    {
        // This test verifies that the CustomerUserId property is accessible 
        // through the ProductionOrders entity, ensuring the domain model 
        // and database context are properly synchronized.
        
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var exception = await Record.ExceptionAsync(async () =>
        {
            var exists = await context.ProductionOrders
                .Select(o => o.CustomerUserId)
                .FirstOrDefaultAsync();
        });

        Assert.Null(exception);
    }
}
