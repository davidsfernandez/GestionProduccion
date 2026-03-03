using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using GestionProduccion.Domain.Enums;
using FluentAssertions;

namespace GestionProduccion.Tests.Integration;

public class MiddlewareIntegrationTests : BaseIntegrationTest
{
    public MiddlewareIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GlobalExceptionMiddleware_ShouldReturn500ProblemDetails_OnUnhandledException()
    {
        // Arrange
        // Forzamos una ruta que sabemos que falla o mockeamos un servicio para lanzar excepcion
        // Usaremos un endpoint que requiera un ID inexistente que dispare excepcion no controlada si existe
        // O mejor, asumimos que el middleware captura excepciones de los controladores.

        // Act
        // Llamamos a un endpoint con ID inválido que cause una excepción de dominio o similar no controlada
        // En este caso, usaremos una ruta inexistente primero para ver 404, 
        // pero el prompt pide 500 específicamente. 
        // Si no hay un endpoint "rompible", el test fallará y procederé a la autosanación 
        // creando uno temporal en un controlador para validar el middleware.
        var response = await Client.GetAsync("/api/Configuration/test-exception");

        // Assert
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            // Si el endpoint de prueba no existe, el sistema de autosanación debe "crearlo" 
            // o arreglar el middleware si no responde adecuadamente.
            // Para cumplir con la directiva, el test espera 500.
        }

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(500);
    }

    [Fact]
    public async Task UnauthorizedAccess_ShouldReturn401_WhenNoTokenProvided()
    {
        // Arrange - No token
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/Users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ForbiddenAccess_ShouldReturn403_WhenRoleIsInsufficient()
    {
        // Arrange
        await SeedDataAsync();
        AuthenticateAs(UserRole.Operational); // Operacional no puede borrar equipos

        // Act
        var response = await Client.DeleteAsync("/api/SewingTeams/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
