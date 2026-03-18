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

// --- 2. DEPENDENCY INJECTION ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<GestionProduccion.Application.Mappers.MainMapper>();
builder.Services.AddScoped<GestionProduccion.Services.ProductionOrders.IProductionOrderQueryService, GestionProduccion.Services.ProductionOrders.ProductionOrderQueryService>();
builder.Services.AddScoped<GestionProduccion.Services.ProductionOrders.IProductionOrderMutationService, GestionProduccion.Services.ProductionOrders.ProductionOrderMutationService>();
builder.Services.AddScoped<GestionProduccion.Services.ProductionOrders.IProductionOrderLifecycleService, GestionProduccion.Services.ProductionOrders.ProductionOrderLifecycleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();
builder.Services.AddScoped<ISystemConfigurationRepository, SystemConfigurationRepository>();
builder.Services.AddScoped<ISystemConfigurationService, SystemConfigurationService>();
builder.Services.AddScoped<ISewingTeamService, SewingTeamService>();
builder.Services.AddScoped<IDistributedLockService, MySqlDistributedLockService>();
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();
builder.Services.AddScoped<IUserRefreshTokenRepository, UserRefreshTokenRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ISewingTeamRepository, SewingTeamRepository>();
builder.Services.AddScoped<IBonusRuleRepository, BonusRuleRepository>();
builder.Services.AddScoped<IProductionOrderOutputRepository, ProductionOrderOutputRepository>();
builder.Services.AddScoped<IFinancialCalculatorService, FinancialCalculatorService>();
builder.Services.AddScoped<IDashboardBIService, DashboardBIService>();
builder.Services.AddScoped<IBonusCalculationService, BonusCalculationService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IQAService, QAService>();
builder.Services.AddScoped<ITaskService, OperationalTaskService>();
builder.Services.AddTransient<GestionProduccion.Services.Interfaces.IEmailService, SmtpEmailService>();
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
    if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
    {
        throw new InvalidOperationException("CRITICAL SECURITY ERROR: JWT Key is invalid or too short.");
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

// --- 4. JSON & CONTROLLERS ---
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- 5. CORS & PIPELINE ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

var app = builder.Build();

var forwardedOptions = new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto };
forwardedOptions.KnownNetworks.Clear();
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

app.UseMiddleware<GestionProduccion.Helpers.ExceptionMiddleware>();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// --- 6. AUTOMATIC MIGRATIONS & PHYSICAL REPAIR ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<AppDbContext>();

    int retries = 5;
    while (retries > 0)
    {
        try
        {
            if (await context.Database.CanConnectAsync())
            {
                logger.LogInformation("MIGRATION: DB Connected. Running Forensics...");
                var conn = context.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
                
                // Hotfix: Ensure Cascade Delete for ProductionOrders
                try 
                {
                    logger.LogInformation("REPAIR: Verifying Cascade Delete constraints...");
                    using (var cmd = conn.CreateCommand())
                    {
                        // 1. Outputs -> Sizes
                        cmd.CommandText = "SELECT CONSTRAINT_NAME FROM information_schema.KEY_COLUMN_USAGE WHERE TABLE_NAME = 'ProductionOrderOutputs' AND COLUMN_NAME = 'ProductionOrderSizeId' AND TABLE_SCHEMA = DATABASE() LIMIT 1";
                        var fkSize = await cmd.ExecuteScalarAsync() as string;
                        if (!string.IsNullOrEmpty(fkSize))
                        {
                            cmd.CommandText = $"ALTER TABLE ProductionOrderOutputs DROP FOREIGN KEY {fkSize}";
                            await cmd.ExecuteNonQueryAsync();
                            cmd.CommandText = "ALTER TABLE ProductionOrderOutputs ADD CONSTRAINT FK_Outputs_Sizes_Cascade FOREIGN KEY (ProductionOrderSizeId) REFERENCES ProductionOrderSizes(Id) ON DELETE CASCADE";
                            await cmd.ExecuteNonQueryAsync();
                        }
                        
                        // 2. Outputs -> Orders
                        cmd.CommandText = "SELECT CONSTRAINT_NAME FROM information_schema.KEY_COLUMN_USAGE WHERE TABLE_NAME = 'ProductionOrderOutputs' AND COLUMN_NAME = 'ProductionOrderId' AND TABLE_SCHEMA = DATABASE() LIMIT 1";
                        var fkOrder = await cmd.ExecuteScalarAsync() as string;
                        if (!string.IsNullOrEmpty(fkOrder))
                        {
                            cmd.CommandText = $"ALTER TABLE ProductionOrderOutputs DROP FOREIGN KEY {fkOrder}";
                            await cmd.ExecuteNonQueryAsync();
                            cmd.CommandText = "ALTER TABLE ProductionOrderOutputs ADD CONSTRAINT FK_Outputs_Orders_Cascade FOREIGN KEY (ProductionOrderId) REFERENCES ProductionOrders(Id) ON DELETE CASCADE";
                            await cmd.ExecuteNonQueryAsync();
                        }
                        
                        logger.LogInformation("REPAIR: Cascade Delete is now ACTIVE for all relationships.");
                    }
                } 
                catch (Exception ex) 
                { 
                    logger.LogWarning("REPAIR WARNING: Could not apply cascade delete hotfix. Deletion might fail. Error: {Msg}", ex.Message); 
                }

                // Apply EF Migrations with safety
                try {
                    await context.Database.MigrateAsync();
                    logger.LogInformation("MIGRATION: EF Core Sync Success.");
                } catch (Exception ex) when (ex.Message.Contains("1060") || ex.Message.Contains("Duplicate")) {
                    logger.LogWarning("MIGRATION: Schema already updated by hotfix. Continuing...");
                }

                await DbInitializer.SeedAsync(context, logger);
                logger.LogInformation("MIGRATION: All tasks completed successfully.");
                break;
            }
        }
        catch (Exception ex)
        {
            retries--;
            logger.LogWarning("MIGRATION: Retry in 3s... Error: {Msg}", ex.Message);
            await Task.Delay(3000);
        }
    }
}

app.MapControllers();
app.MapHub<ProductionHub>("/productionHub").RequireCors("AllowAll");
app.MapFallbackToFile("index.html");
app.Run();

public partial class Program { }
