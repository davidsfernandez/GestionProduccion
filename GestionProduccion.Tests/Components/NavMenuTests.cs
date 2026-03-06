/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

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
        // 'Ajustes do Sistema', 'UsuÃ¡rios', 'CatÃ¡logo de Produtos'
        
        // Ajustes do Sistema
        cut.FindAll("a").Where(a => a.TextContent.Contains("Ajustes do Sistema")).Should().BeEmpty("Settings link should be hidden for Operational role");

        // UsuÃ¡rios
        cut.FindAll("a").Where(a => a.TextContent.Contains("UsuÃ¡rios")).Should().BeEmpty("Users link should be hidden for Operational role");

        // CatÃ¡logo de Produtos
        cut.FindAll("a").Where(a => a.TextContent.Contains("CatÃ¡logo de Produtos")).Should().BeEmpty("Catalog link should be hidden for Operational role");
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
        cut.FindAll("a").Where(a => a.TextContent.Contains("UsuÃ¡rios")).Should().NotBeEmpty("Users link should be visible for Admin");
        cut.FindAll("a").Where(a => a.TextContent.Contains("CatÃ¡logo de Produtos")).Should().NotBeEmpty("Catalog link should be visible for Admin");
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
        // Ranking & BÃ´nus, QA should NOT exist
        cut.FindAll("a").Where(a => a.TextContent.Contains("Ranking & BÃ´nus")).Should().BeEmpty("Bonus module should not be visible");
        cut.FindAll("a").Where(a => a.TextContent.Contains("QA")).Should().BeEmpty("QA module should not be visible");
    }
}


