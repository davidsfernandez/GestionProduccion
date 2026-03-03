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
        // 'Ajustes do Sistema', 'Usuários', 'Catálogo de Produtos'
        
        // Ajustes do Sistema
        cut.FindAll("a").Where(a => a.TextContent.Contains("Ajustes do Sistema")).Should().BeEmpty("Settings link should be hidden for Operational role");

        // Usuários
        cut.FindAll("a").Where(a => a.TextContent.Contains("Usuários")).Should().BeEmpty("Users link should be hidden for Operational role");

        // Catálogo de Produtos
        cut.FindAll("a").Where(a => a.TextContent.Contains("Catálogo de Produtos")).Should().BeEmpty("Catalog link should be hidden for Operational role");
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
        cut.FindAll("a").Where(a => a.TextContent.Contains("Ajustes do Sistema")).Should().NotBeEmpty("Settings link should be visible for Admin");
        cut.FindAll("a").Where(a => a.TextContent.Contains("Usuários")).Should().NotBeEmpty("Users link should be visible for Admin");
        cut.FindAll("a").Where(a => a.TextContent.Contains("Catálogo de Produtos")).Should().NotBeEmpty("Catalog link should be visible for Admin");
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
        // Ranking & Bônus, QA should NOT exist
        cut.FindAll("a").Where(a => a.TextContent.Contains("Ranking & Bônus")).Should().BeEmpty("Bonus module should not be visible");
        cut.FindAll("a").Where(a => a.TextContent.Contains("QA")).Should().BeEmpty("QA module should not be visible");
    }
}
