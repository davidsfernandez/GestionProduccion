using Xunit;
using GestionProduccion.Data;
using Microsoft.EntityFrameworkCore;
using GestionProduccion.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace GestionProduccion.Tests.Integration;

public class DatabaseSchemaTests : BaseIntegrationTest
{
    public DatabaseSchemaTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task VerifySchema_ProductionOrderHasCustomerUserId()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Intenta realizar una consulta que involucre CustomerUserId
        try 
        {
            var exists = await context.ProductionOrders
                .Select(o => o.CustomerUserId)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Assert.Fail($"La columna CustomerUserId no parece existir en la base de datos o el esquema está roto: {ex.Message}");
        }
    }
}
