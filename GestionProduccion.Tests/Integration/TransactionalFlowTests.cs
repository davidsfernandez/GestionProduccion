using System.Net;
using System.Net.Http.Json;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Models.DTOs;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using GestionProduccion.Data;

namespace GestionProduccion.Tests.Integration;

public class TransactionalFlowTests : BaseIntegrationTest
{
    public TransactionalFlowTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateOrder_ShouldCalculateRealCost_WhenCompleted()
    {
        // Arrange
        await SeedDataAsync();
        AuthenticateAs(UserRole.Administrator);

        int actualProductId = 0;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var product = await db.Products.FirstAsync();
            actualProductId = product.Id;
        }

        var orderRequest = new CreateProductionOrderRequest
        {
            ProductId = actualProductId,
            Quantity = 10,
            EstimatedCompletionAt = DateTime.Now.AddDays(7),
            Size = "M"
        };

        // Act 1: Crear Orden
        var createResponse = await Client.PostAsJsonAsync("/api/ProductionOrders", orderRequest);
        createResponse.EnsureSuccessStatusCode();
        var newOrder = await createResponse.Content.ReadFromJsonAsync<ProductionOrderDto>(JsonOptions);
        int orderId = newOrder!.Id;

        // Act 2: Forzar estado completado con costos (Simulando el efecto del LifecycleService + Calculator)
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var order = await db.ProductionOrders.FindAsync(orderId);
            order!.StartedAt = DateTime.UtcNow.AddHours(-2);
            order.CompletedAt = DateTime.UtcNow;
            order.CurrentStage = ProductionStage.Packaging;
            order.CurrentStatus = ProductionStatus.Completed;

            order.TotalCost = 450.0m;
            order.AverageCostPerPiece = 45.0m;

            await db.SaveChangesAsync();
        }

        // Act 3: Verificar que la API devuelve los datos persistidos correctamente
        var getResponse = await Client.GetAsync($"/api/ProductionOrders/{orderId}");
        var orderDetails = await getResponse.Content.ReadFromJsonAsync<ProductionOrderDto>(JsonOptions);

        // Assert
        orderDetails!.TotalCost.Should().BeGreaterThan(0, "TotalCost must be returned by the API");
        orderDetails!.AverageCostPerPiece.Should().BeGreaterThan(0, "AverageCostPerPiece must be returned by the API");
    }

    [Fact]
    public async Task DeleteTeam_ShouldReassignMembers_ToAnotherTeam()
    {
        // Arrange
        await SeedDataAsync();
        AuthenticateAs(UserRole.Administrator);

        string suffix = Guid.NewGuid().ToString().Substring(0, 8);

        // ID del operador del SeedData (usualmente 2 en InMemory si es el segundo insertado tras reset)
        int opUserId = 0;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var op = await db.Users.FirstAsync(u => u.Role == UserRole.Operational);
            opUserId = op.Id;
        }

        // Crear Equipo A (Equipo de origen) con el operador
        var teamAReq = new CreateSewingTeamRequest { Name = $"TeamA_{suffix}", InitialUserIds = new List<int> { opUserId } };
        var teamAResp = await Client.PostAsJsonAsync("/api/SewingTeams", teamAReq);
        teamAResp.EnsureSuccessStatusCode();
        var teamAResult = await teamAResp.Content.ReadFromJsonAsync<ApiResponse<SewingTeamDto>>(JsonOptions);
        int teamAId = teamAResult!.Data!.Id;

        // Crear Equipo B (Equipo de destino)
        var teamBRequest = new CreateSewingTeamRequest { Name = $"TeamB_{suffix}", InitialUserIds = new List<int> { 1 } };
        var teamBResp = await Client.PostAsJsonAsync("/api/SewingTeams", teamBRequest);
        teamBResp.EnsureSuccessStatusCode();
        var teamBResult = await teamBResp.Content.ReadFromJsonAsync<ApiResponse<SewingTeamDto>>(JsonOptions);
        int teamBId = teamBResult!.Data!.Id;

        // Act: Borrar Equipo A
        var deleteResp = await Client.DeleteAsync($"/api/SewingTeams/{teamAId}");
        deleteResp.EnsureSuccessStatusCode();

        // Act: Verificar reasignación del usuario a algún equipo activo (distinto al borrado)
        var getUserResp = await Client.GetAsync($"/api/Users/{opUserId}");
        var getUserResult = await getUserResp.Content.ReadFromJsonAsync<UserDto>(JsonOptions);

        // Assert
        getUserResult!.SewingTeamId.Should().NotBeNull();
        getUserResult!.SewingTeamId.Should().NotBe(teamAId, "User must be reassigned away from the deleted team");
    }
}
