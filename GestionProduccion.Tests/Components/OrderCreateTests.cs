using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using GestionProduccion.Client.Pages;
using GestionProduccion.Client.Services;
using GestionProduccion.Client.Services.ProductionOrders;
using GestionProduccion.Models.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace GestionProduccion.Tests.Components;

public class OrderCreateTests : TestContext
{
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly Mock<IProductionOrderMutationClient> _mockMutationClient;
    private readonly Mock<ISewingTeamClient> _mockTeamClient;
    private readonly Mock<IProductClient> _mockProductClient;
    private readonly Mock<IProductionOrderLifecycleClient> _mockLifecycleClient;
    private readonly Mock<IProductionOrderQueryClient> _mockQueryClient;

    public OrderCreateTests()
    {
        this.AddTestAuthorization().SetAuthorized("Admin").SetRoles("Administrator");
        JSInterop.Mode = JSRuntimeMode.Loose;

        _mockHttpHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_mockHttpHandler.Object) { BaseAddress = new Uri("http://localhost/") };
        Services.AddSingleton(httpClient);
        Services.AddSingleton(new ToastService());

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
        Services.AddSingleton(jsonOptions);

        _mockMutationClient = new Mock<IProductionOrderMutationClient>();
        Services.AddSingleton(_mockMutationClient.Object);

        _mockTeamClient = new Mock<ISewingTeamClient>();
        Services.AddSingleton(_mockTeamClient.Object);

        _mockProductClient = new Mock<IProductClient>();
        Services.AddSingleton(_mockProductClient.Object);

        _mockLifecycleClient = new Mock<IProductionOrderLifecycleClient>();
        Services.AddSingleton(_mockLifecycleClient.Object);

        _mockQueryClient = new Mock<IProductionOrderQueryClient>();
        Services.AddSingleton(_mockQueryClient.Object);
    }

    [Fact]
    public void OrderCreate_ShouldPopulateSizesDropdown_Statically()
    {
        // Arrange
        var productsJson = JsonSerializer.Serialize(new ApiResponse<List<ProductDto>> { Success = true, Data = new List<ProductDto>() }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { new JsonStringEnumConverter() } });
        var teamsJson = JsonSerializer.Serialize(new ApiResponse<List<SewingTeamDto>> { Success = true, Data = new List<SewingTeamDto>() }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { new JsonStringEnumConverter() } });
        var usersJson = JsonSerializer.Serialize(new List<UserDto>(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { new JsonStringEnumConverter() } });

        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                var url = request.RequestUri!.ToString();

                if (url.Contains("api/Products"))
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(productsJson) };
                }
                if (url.Contains("api/SewingTeams"))
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(teamsJson) };
                }
                if (url.Contains("api/Users"))
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(usersJson) };
                }

                // Fallback for any other call to avoid 404 if component makes extra calls
                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("{}") };
            });

        // Act
        var cut = RenderComponent<OrderCreate>();

        // Assert
        cut.WaitForState(() => cut.FindAll("select#size").Count > 0);

        var select = cut.Find("select#size");
        var options = select.Children;

        options.Should().Contain(e => e.TextContent == "PP");
        options.Should().Contain(e => e.TextContent == "M");
        options.Should().Contain(e => e.TextContent == "38");
    }
}
