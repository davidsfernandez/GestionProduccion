using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using GestionProduccion.Data;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionProduccion.Tests.Integration;

public abstract class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    protected readonly HttpClient Client;
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly IConfiguration Configuration;
    protected readonly JsonSerializerOptions JsonOptions;

    private readonly string _dbName;

    protected BaseIntegrationTest(CustomWebApplicationFactory<Program> factory)
    {
        _dbName = $"Db_{Guid.NewGuid()}";
        Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_dbName));
            });
        });
        Client = Factory.CreateClient();
        Configuration = Factory.Services.GetRequiredService<IConfiguration>();

        JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    protected void AuthenticateAs(UserRole role)
    {
        var token = GenerateJwtToken(role);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private string GenerateJwtToken(UserRole role)
    {
        var jwtSettings = Configuration.GetSection("Jwt");
        var secretKey = Configuration["Jwt:Key"];

        if (string.IsNullOrEmpty(secretKey) || secretKey == "REPLACE_WITH_SECURE_KEY_IN_ENVIRONMENT_VARIABLES")
        {
            secretKey = "SUPER_SECRET_KEY_FOR_GESTION_PRODUCCION_2024_!@#";
        }

        var key = Encoding.ASCII.GetBytes(secretKey);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = jwtSettings["Issuer"] ?? "GestionProduccion",
            Audience = jwtSettings["Audience"] ?? "GestionProduccionAPI"
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    protected async Task SeedDataAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // NO llamar a EnsureDeleted aquí porque borraría la DB que acabamos de crear en el constructor para este test
        db.Database.EnsureCreated();

        // Limpiar tablas específicas si es necesario, pero InMemory con nombre único ya debería estar vacío

        // 1. Producto
        var product = new Product
        {
            Name = "Camiseta Test",
            InternalCode = "CAM-001",
            FabricType = "Algodón",
            MainSku = "CAM001-TEST",
            EstimatedSalePrice = 100.0m,
            AverageProductionTimeMinutes = 30.0
        };
        db.Products.Add(product);

        // 2. Equipos
        var team1 = new SewingTeam { Name = "Equipo Integración 1", IsActive = true };
        db.SewingTeams.Add(team1);
        var team2 = new SewingTeam { Name = "Equipo Integración 2", IsActive = true };
        db.SewingTeams.Add(team2);

        // 3. Usuarios
        var admin = new User
        {
            FullName = "Admin Test",
            Email = "admin@integration.com",
            PasswordHash = "BCRYPT_HASH_HERE",
            Role = UserRole.Administrator,
            IsActive = true
        };
        db.Users.Add(admin);

        var op = new User
        {
            FullName = "Operator Test",
            Email = "operator@integration.com",
            PasswordHash = "BCRYPT_HASH_HERE",
            Role = UserRole.Operational,
            IsActive = true
        };
        db.Users.Add(op);

        // 4. Configuración
        db.SystemConfigurations.Add(new SystemConfiguration
        {
            Key = "MainConfig",
            OperationalHourlyCost = 45.0m
        });

        await db.SaveChangesAsync();
    }
}
