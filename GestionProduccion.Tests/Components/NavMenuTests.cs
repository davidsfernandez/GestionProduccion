using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using GestionProduccion.Client.Layout;
using GestionProduccion.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using Xunit;

namespace GestionProduccion.Tests.Components;

public class NavMenuTests : TestContext
{
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;

    public NavMenuTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_mockHttpHandler.Object) { BaseAddress = new Uri("http://localhost/") };
        Services.AddSingleton(httpClient);
        Services.AddSingleton(new UserStateService());
    }

    [Fact]
    public void NavMenu_ShouldHideAdminLinks_ForOperationalUser()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("Operator");
        authContext.SetRoles("Operational");

        // Act
        var cut = RenderComponent<NavMenu>();

        // Assert - Verify via CSS selectors that links do NOT render
        // 'Configurações', 'Usuários', 'Catálogo'
        // We check for anchor tags with specific text or href

        // Configurações
        cut.FindAll("a").Where(a => a.TextContent.Contains("Configurações")).Should().BeEmpty("Settings link should be hidden for Operational role");

        // Usuários
        cut.FindAll("a").Where(a => a.TextContent.Contains("Usuários")).Should().BeEmpty("Users link should be hidden for Operational role");

        // Catálogo
        cut.FindAll("a").Where(a => a.TextContent.Contains("Catálogo")).Should().BeEmpty("Catalog link should be hidden for Operational role");
    }

    [Fact]
    public void NavMenu_ShouldShowAllLinks_ForAdministrator()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("Admin");
        authContext.SetRoles("Administrator");

        // Act
        var cut = RenderComponent<NavMenu>();

        // Assert
        cut.FindAll("a").Where(a => a.TextContent.Contains("Configurações")).Should().NotBeEmpty("Settings link should be visible for Admin");
        cut.FindAll("a").Where(a => a.TextContent.Contains("Usuários")).Should().NotBeEmpty("Users link should be visible for Admin");
        // Check catalog if it exists in Admin view (prompt implies it should be visible for Admin or maybe Operational too? Prompt says "Simula un usuario con rol 'Operacional'... enlaces... NO se rendericen". So for Admin they SHOULD render.)
        cut.FindAll("a").Where(a => a.TextContent.Contains("Catálogo")).Should().NotBeEmpty("Catalog link should be visible for Admin");
    }

    [Fact]
    public void NavMenu_ShouldHideOutOfScopeModules_ForAnyUser()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("Admin");
        authContext.SetRoles("Administrator");

        // Act
        var cut = RenderComponent<NavMenu>();

        // Assert - Poda Visual
        // Bonos, QA should NOT exist
        cut.FindAll("a").Where(a => a.TextContent.Contains("Bônus")).Should().BeEmpty("Bonus module should not be visible");
        cut.FindAll("a").Where(a => a.TextContent.Contains("QA")).Should().BeEmpty("QA module should not be visible");
    }
}
