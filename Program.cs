/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

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
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// --- 0. PDF ENGINE LICENSE ---
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// --- 1. DATABASE CONFIGURATION ---
var dbServer = builder.Configuration["DB_SERVER"];
var dbName = builder.Configuration["DB_NAME"];
var dbUser = builder.Configuration["DB_USER"];
var dbPass = builder.Configuration["DB_PASS"];

string? connectionString;
if (!string.IsNullOrEmpty(dbServer) && !string.IsNullOrEmpty(dbName))
{
    connectionString = $"server={dbServer};port=3306;database={dbName};user={dbUser};password={dbPass};AllowUserVariables=true;ConvertZeroDateTime=true;";
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

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
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
            mysqlOptions => mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null)),
        poolSize: 128
    );
}

// --- 2. DEPENDENCY INJECTION (Armored) ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<GestionProduccion.Application.Mappers.MainMapper>();
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
builder.Services.AddScoped<IDistributedLockService, MySqlDistributedLockService>();
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();
builder.Services.AddScoped<GestionProduccion.Domain.Interfaces.Repositories.IUserRefreshTokenRepository, GestionProduccion.Data.Repositories.UserRefreshTokenRepository>();
builder.Services.AddScoped<GestionProduccion.Domain.Interfaces.Repositories.IPasswordResetTokenRepository, GestionProduccion.Data.Repositories.PasswordResetTokenRepository>();
builder.Services.AddMemoryCache(); // TV Dashboard optimization
builder.Services.AddScoped<GestionProduccion.Domain.Interfaces.Repositories.IProductRepository, GestionProduccion.Data.Repositories.ProductRepository>();
builder.Services.AddScoped<GestionProduccion.Domain.Interfaces.Repositories.ISewingTeamRepository, GestionProduccion.Data.Repositories.SewingTeamRepository>();
builder.Services.AddScoped<GestionProduccion.Domain.Interfaces.Repositories.IBonusRuleRepository, GestionProduccion.Data.Repositories.BonusRuleRepository>();
builder.Services.AddScoped<GestionProduccion.Domain.Interfaces.Repositories.IProductionOrderOutputRepository, GestionProduccion.Data.Repositories.ProductionOrderOutputRepository>();
builder.Services.AddScoped<IFinancialCalculatorService, FinancialCalculatorService>();
builder.Services.AddScoped<IDashboardBIService, DashboardBIService>();
builder.Services.AddScoped<IBonusCalculationService, BonusCalculationService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IQAService, QAService>();
builder.Services.AddScoped<ITaskService, OperationalTaskService>();
builder.Services.AddTransient<GestionProduccion.Services.Interfaces.IEmailService, GestionProduccion.Services.SmtpEmailService>();
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

    if (string.IsNullOrEmpty(jwtKey) || 
        jwtKey == "REPLACE_WITH_SECURE_KEY_IN_ENVIRONMENT_VARIABLES" ||
        jwtKey.Length < 32)
    {
        throw new InvalidOperationException("CRITICAL SECURITY ERROR: JWT Key is missing, insecure, or too short (min 32 chars). System startup aborted.");
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

        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 1000000,
                    QueueLimit = 0,
                    Window = TimeSpan.FromMinutes(1)
                }));

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
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
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

// --- 8. CORS REPAIR ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// --- 8. MIDDLEWARE PIPELINE ---
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedOptions.KnownNetworks.Clear();
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

app.UseMiddleware<GestionProduccion.Helpers.ExceptionMiddleware>();

if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("ENABLE_SWAGGER") == "true")
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseWebAssemblyDebugging();
    }
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    await next();
});

app.UseResponseCompression();

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
{
    app.UseHttpsRedirection();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseRateLimiter();
}

app.UseAuthentication();
app.UseAuthorization();

// --- 9. AUTOMATIC MIGRATIONS & SEEDING ---
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var context = services.GetRequiredService<AppDbContext>();

        int retries = 5;
        int delaySeconds = 3;

        logger.LogInformation("MIGRATION: Starting process...");

        while (retries > 0)
        {
            try
            {
                if (await context.Database.CanConnectAsync())
                {
                    logger.LogInformation("MIGRATION: Database connected. Running hotfixes...");

                    // 1. EMERGENCY HOTFIX: Column existence checks
                    try 
                    {
                        var conn = context.Database.GetDbConnection();
                        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
                        using (var cmd = conn.CreateCommand())
                        {
                            // QA Defect Responsible
                            cmd.CommandText = "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME = 'QADefects' AND COLUMN_NAME = 'ResponsibleUserId' AND TABLE_SCHEMA = DATABASE()";
                            if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 0)
                            {
                                logger.LogCritical("CRITICAL: Column 'ResponsibleUserId' MISSING! Forcing ALTER...");
                                cmd.CommandText = "ALTER TABLE QADefects ADD COLUMN ResponsibleUserId INT NULL, ADD CONSTRAINT FK_QADefects_Users_ResponsibleUserId FOREIGN KEY (ResponsibleUserId) REFERENCES Users(Id) ON DELETE SET NULL";
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // Atomic Mode
                            cmd.CommandText = "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME = 'BonusRules' AND COLUMN_NAME = 'IsAtomicMode' AND TABLE_SCHEMA = DATABASE()";
                            if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 0)
                            {
                                logger.LogCritical("CRITICAL: Column 'IsAtomicMode' MISSING! Forcing ALTER...");
                                cmd.CommandText = "ALTER TABLE BonusRules ADD COLUMN IsAtomicMode TINYINT(1) NOT NULL DEFAULT 0";
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // Default Bonus
                            cmd.CommandText = "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'DefaultBonusPerPiece' AND TABLE_SCHEMA = DATABASE()";
                            if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 0)
                            {
                                logger.LogCritical("CRITICAL: Column 'DefaultBonusPerPiece' MISSING! Forcing ALTER...");
                                cmd.CommandText = "ALTER TABLE Products ADD COLUMN DefaultBonusPerPiece DECIMAL(18,2) NOT NULL DEFAULT 0";
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // Applied Bonus
                            cmd.CommandText = "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME = 'ProductionOrders' AND COLUMN_NAME = 'AppliedBonusPerPiece' AND TABLE_SCHEMA = DATABASE()";
                            if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 0)
                            {
                                logger.LogCritical("CRITICAL: Column 'AppliedBonusPerPiece' MISSING! Forcing ALTER...");
                                cmd.CommandText = "ALTER TABLE ProductionOrders ADD COLUMN AppliedBonusPerPiece DECIMAL(18,2) NOT NULL DEFAULT 0";
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // Daily Goal
                            cmd.CommandText = "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME = 'SystemConfigurations' AND COLUMN_NAME = 'DailyGoal' AND TABLE_SCHEMA = DATABASE()";
                            if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 0)
                            {
                                logger.LogCritical("CRITICAL: Column 'DailyGoal' MISSING! Forcing ALTER...");
                                cmd.CommandText = "ALTER TABLE SystemConfigurations ADD COLUMN DailyGoal INT NOT NULL DEFAULT 500";
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // Production Order Archiving
                            cmd.CommandText = "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME = 'ProductionOrders' AND COLUMN_NAME = 'IsArchived' AND TABLE_SCHEMA = DATABASE()";
                            if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 0)
                            {
                                logger.LogCritical("CRITICAL: Column 'IsArchived' MISSING! Forcing ALTER...");
                                cmd.CommandText = "ALTER TABLE ProductionOrders ADD COLUMN IsArchived TINYINT(1) NOT NULL DEFAULT 0";
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                    }
                    catch (Exception ex) { logger.LogWarning("HOTFIX SKIPPED: {Msg}", ex.Message); }

                    // 2. APPLY MIGRATIONS
                    try
                    {
                        var pending = await context.Database.GetPendingMigrationsAsync();
                        if (pending.Any())
                        {
                            logger.LogInformation("MIGRATION: Applying {Count} migrations...", pending.Count());
                            await context.Database.MigrateAsync();
                        }
                    }
                    catch (Exception ex) 
                    {
                        var msg = ex.Message + (ex.InnerException?.Message ?? "");
                        if (msg.Contains("1060") || msg.Contains("Duplicate column name"))
                        {
                            logger.LogWarning("MIGRATION: Columns already exist (Handled). Proceeding to seed...");
                        }
                        else 
                        {
                            logger.LogError("MIGRATION FATAL ERROR: {Msg}", ex.Message);
                            throw; 
                        }
                    }

                    // 3. SEEDING
                    await DbInitializer.SeedAsync(context, logger);
                    logger.LogInformation("MIGRATION: All tasks completed.");
                    break;
                }
                else throw new Exception("CanConnectAsync failed");
            }
            catch (Exception ex)
            {
                retries--;
                logger.LogWarning("MIGRATION: Failed attempt. Retrying in {Delay}s... Error: {Msg}", delaySeconds, ex.Message);
                await Task.Delay(delaySeconds * 1000);
            }
        }
    }
}

app.MapControllers();
app.MapRazorPages();
app.UseWebSockets();
app.MapHub<ProductionHub>("/productionHub").RequireCors("AllowAll");
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }
