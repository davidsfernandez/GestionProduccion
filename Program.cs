using GestionProduccion.Services.ProductionOrders;
using GestionProduccion.Data;
using Microsoft.EntityFrameworkCore;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Services;
using GestionProduccion.Hubs;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Data.Repositories;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// --- 0. PDF ENGINE LICENSE ---
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// --- 1. DATABASE CONFIGURATION ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36))));
}
else
{
    builder.Services.AddDbContextPool<AppDbContext>(options =>
        options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)),
            mysqlOptions => mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null)),
        poolSize: 128
    );
}

// --- 2. DEPENDENCY INJECTION (Armored) ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<GestionProduccion.Services.ProductionOrders.IProductionOrderQueryService, GestionProduccion.Services.ProductionOrders.ProductionOrderQueryService>();
builder.Services.AddScoped<GestionProduccion.Services.ProductionOrders.IProductionOrderMutationService, GestionProduccion.Services.ProductionOrders.ProductionOrderMutationService>();
builder.Services.AddScoped<GestionProduccion.Services.ProductionOrders.IProductionOrderLifecycleService, GestionProduccion.Services.ProductionOrders.ProductionOrderLifecycleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();
builder.Services.AddScoped<GestionProduccion.Domain.Interfaces.Repositories.IUserRepository, GestionProduccion.Data.Repositories.UserRepository>();
builder.Services.AddScoped<GestionProduccion.Domain.Interfaces.Repositories.IProductionOrderRepository, GestionProduccion.Data.Repositories.ProductionOrderRepository>();
builder.Services.AddScoped<GestionProduccion.Domain.Interfaces.Repositories.ISystemConfigurationRepository, GestionProduccion.Data.Repositories.SystemConfigurationRepository>();
builder.Services.AddScoped<ISystemConfigurationService, SystemConfigurationService>();
builder.Services.AddScoped<ISewingTeamService, SewingTeamService>();
builder.Services.AddScoped<GestionProduccion.Domain.Interfaces.Repositories.IUserRefreshTokenRepository, GestionProduccion.Data.Repositories.UserRefreshTokenRepository>();
builder.Services.AddScoped<GestionProduccion.Domain.Interfaces.Repositories.IPasswordResetTokenRepository, GestionProduccion.Data.Repositories.PasswordResetTokenRepository>();
builder.Services.AddMemoryCache(); // TV Dashboard optimization
builder.Services.AddScoped<GestionProduccion.Domain.Interfaces.Repositories.IProductRepository, GestionProduccion.Data.Repositories.ProductRepository>();
builder.Services.AddScoped<GestionProduccion.Domain.Interfaces.Repositories.ISewingTeamRepository, GestionProduccion.Data.Repositories.SewingTeamRepository>();
builder.Services.AddScoped<GestionProduccion.Domain.Interfaces.Repositories.IBonusRuleRepository, GestionProduccion.Data.Repositories.BonusRuleRepository>();
builder.Services.AddScoped<IFinancialCalculatorService, FinancialCalculatorService>();
builder.Services.AddScoped<IDashboardBIService, DashboardBIService>();
builder.Services.AddScoped<IBonusCalculationService, BonusCalculationService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IQAService, QAService>();
builder.Services.AddScoped<ITaskService, OperationalTaskService>();
builder.Services.AddTransient<GestionProduccion.Services.Interfaces.IEmailService, GestionProduccion.Services.SmtpEmailService>()
;
builder.Services.AddSignalR();

// --- 3. AUTHENTICACION & JWT ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"];
    var isProduction = builder.Environment.IsProduction();

    if (string.IsNullOrEmpty(jwtKey) || jwtKey == "REPLACE_WITH_SECURE_KEY_IN_ENVIRONMENT_VARIABLES")
    {
        if (isProduction)
            throw new InvalidOperationException("CRITICAL: JWT Key is missing in Production environment!");

        jwtKey = "SUPER_SECRET_KEY_FOR_GESTION_PRODUCCION_2024_!@#"; // Dev fallback
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "GestionProduccion",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "GestionProduccionAPI",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// --- 4. RATE LIMITING (Security) ---
bool isTesting = builder.Environment.IsEnvironment("Testing");
if (!isTesting)
{
    builder.Services.AddRateLimiter(options =>
    {
        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = 429;
            await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken: token);
        };

        // Global Policy: 1000 requests per minute per IP (increased for production stability)
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 1000,
                    QueueLimit = 0,
                    Window = TimeSpan.FromMinutes(1)
                }));

        // Login Policy: 10 requests per minute per IP
        options.AddPolicy("LoginPolicy", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 10,
                    QueueLimit = 0,
                    Window = TimeSpan.FromMinutes(1)
                }));
    });
}

// --- 5. VALIDATION ---
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// --- 6. CONTROLLERS & JSON REPAIR ---
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Fix for circular references (Architect Rule 7)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // Fix for Enums as strings (Architect Rule 9)
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // Maintain CamelCase for consistency with JavaScript
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        // Safeguard against nulls (Architect Rule 48)
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddRazorPages();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- 7. RESPONSE COMPRESSION (Infrastructure Optimization) ---
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream", "application/json", "application/wasm" });
});

// --- 8. CORS REPAIR (Architect Rule 11) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// --- 8. MIDDLEWARE PIPELINE (Correct Order) ---
// Enable Swagger in Dev or if explicitly enabled via Env Var (e.g. in Docker)
if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("ENABLE_SWAGGER") == "true")
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage(); // Architect Rule 18
        app.UseWebAssemblyDebugging();
    }
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GestionProduccion.Helpers.ExceptionMiddleware>();

// Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    await next();
});

app.UseResponseCompression();

// Only enforce HTTPS Redirection if NOT running in a container (Docker handles SSL termination usually)
if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
{
    app.UseHttpsRedirection();
}

app.UseBlazorFrameworkFiles(); // Architect Rule 19
app.UseStaticFiles();          // Architect Rule 19
app.UseRouting();

app.UseCors("AllowAll"); // Must be before Auth

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseRateLimiter(); // Apply Rate Limiting
}

app.UseAuthentication();
app.UseAuthorization();

// --- 9. AUTOMATIC MIGRATIONS (Architect Rule 5) ---
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var context = services.GetRequiredService<AppDbContext>();

        int retries = 10;
        while (retries > 0)
        {
            try
            {
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
                    await context.Database.MigrateAsync();
                }

                logger.LogInformation("Database is up to date. Ensuring seed data...");
                await DbInitializer.SeedAsync(context, logger);

                break;
            }
            catch (Exception ex)
            {
                retries--;
                if (retries == 0)
                {
                    logger.LogCritical(ex, "FATAL: Database migration/seed failed after multiple attempts.");
                    throw;
                }
                logger.LogWarning("DB Connection failed or busy. Retrying in 5 seconds... ({Retries} attempts left). Error: {Message}", retries, ex.Message);
                await Task.Delay(5000);
            }
        }
    }
}

app.MapControllers();
app.MapRazorPages();
app.MapHub<ProductionHub>("/productionHub");
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }
