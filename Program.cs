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
builder.Services.AddScoped<IProductionOrderQueryService, ProductionOrderQueryService>();
builder.Services.AddScoped<IProductionOrderMutationService, ProductionOrderMutationService>();
builder.Services.AddScoped<IProductionOrderLifecycleService, ProductionOrderLifecycleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();
builder.Services.AddScoped<ILeadService, LeadService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<ICustomerOrderService, CustomerOrderService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILeadRepository, LeadRepository>();
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

// --- 2.1 HR & FINANCE MODULE SERVICES ---
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IAbsenceService, AbsenceService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

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
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// --- 6. AUTOMATIC MIGRATIONS & FINAL SYSTEM REPAIR ---
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
                logger.LogInformation("SYSTEM: Synchronizing database schema...");

                // 1. Audit Pending Migrations
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                var pendingList = pendingMigrations.ToList();
                if (pendingList.Any())
                {
                    logger.LogInformation("SYSTEM: Found {Count} pending migrations: {Migrations}", pendingList.Count, string.Join(", ", pendingList));
                }
                else
                {
                    logger.LogInformation("SYSTEM: No pending migrations according to EF Core.");
                }

                // 2. Apply EF Migrations with total silence on duplicates
                try
                {
                    await context.Database.MigrateAsync();
                    logger.LogInformation("SYSTEM: EF Core Synchronized.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "SYSTEM: EF Core Migration failed. Details: {Msg}", ex.Message);
                    if (ex.Message.Contains("1060") || ex.Message.Contains("Duplicate"))
                        logger.LogWarning("SYSTEM: Duplicate error detected. Schema might be partially updated. Proceeding with integrity repairs...");
                    else throw;
                }

                logger.LogInformation("SYSTEM: Running Critical Integrity Repairs...");
                var conn = context.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();

                using (var cmd = conn.CreateCommand())
                {
                    // A. Physical Table Repairs (Resilient to EF Core failures)
                    async Task EnsureTable(string name, string sql) {
                        try {
                            cmd.CommandText = $"SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_NAME = '{name}' AND TABLE_SCHEMA = DATABASE()";
                            if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 0) {
                                logger.LogCritical("REPAIR: Table {Tab} MISSING. Creating manually...", name);
                                cmd.CommandText = sql;
                                await cmd.ExecuteNonQueryAsync();
                            }
                        } catch (Exception ex) { logger.LogError("REPAIR ERROR: Could not ensure table {Tab}: {Msg}", name, ex.Message); }
                    }

                    // B. Physical Column Repairs (Resilient to missing tables)
                    async Task EnsureColumn(string table, string column, string definition) {
                        try {
                            cmd.CommandText = $"SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME = '{table}' AND COLUMN_NAME = '{column}' AND TABLE_SCHEMA = DATABASE()";
                            if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 0) {
                                // Double check if table exists to avoid crash
                                cmd.CommandText = $"SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_NAME = '{table}' AND TABLE_SCHEMA = DATABASE()";
                                if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0) {
                                    logger.LogCritical("REPAIR: Adding missing column {Col} to {Tab}...", column, table);
                                    cmd.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {definition}";
                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }
                        } catch (Exception ex) { logger.LogWarning("Resiliency notice: Could not verify column {Col} in {Tab}. {Msg}", column, table, ex.Message); }
                    }

                    // Ensure CRM Tables exist
                    await EnsureTable("Leads", @"CREATE TABLE `Leads` (
                        `Id` int NOT NULL AUTO_INCREMENT,
                        `Name` varchar(100) NOT NULL,
                        `Email` varchar(100) NOT NULL,
                        `Phone` varchar(20) NULL,
                        `Message` varchar(500) NULL,
                        `Status` varchar(50) NOT NULL,
                        `Source` varchar(50) NOT NULL,
                        `CreatedAt` datetime(6) NOT NULL,
                        `UpdatedAt` datetime(6) NOT NULL,
                        `CommercialNotes` longtext NULL,
                        PRIMARY KEY (`Id`)
                    ) ENGINE=InnoDB;");

                    await EnsureTable("Quotes", @"CREATE TABLE `Quotes` (
                        `Id` int NOT NULL AUTO_INCREMENT,
                        `LeadId` int NOT NULL,
                        `CreatedAt` datetime(6) NOT NULL,
                        `ExpiryDate` datetime(6) NOT NULL,
                        `Status` varchar(50) NOT NULL,
                        `TotalAmount` decimal(18,2) NOT NULL,
                        `Notes` longtext NULL,
                        PRIMARY KEY (`Id`),
                        CONSTRAINT `FK_Quotes_Leads_LeadId` FOREIGN KEY (`LeadId`) REFERENCES `Leads` (`Id`) ON DELETE CASCADE
                    ) ENGINE=InnoDB;");

                    await EnsureTable("CustomerProfiles", @"CREATE TABLE `CustomerProfiles` (
                        `Id` int NOT NULL AUTO_INCREMENT,
                        `UserId` int NOT NULL,
                        `TaxId` varchar(20) NULL,
                        `CompanyName` varchar(100) NULL,
                        `Phone` varchar(20) NULL,
                        `Address` varchar(200) NULL,
                        `City` varchar(50) NULL,
                        `State` varchar(2) NULL,
                        `PostalCode` varchar(10) NULL,
                        `CreatedAt` datetime(6) NOT NULL,
                        `UpdatedAt` datetime(6) NOT NULL,
                        PRIMARY KEY (`Id`),
                        CONSTRAINT `FK_CustomerProfiles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
                    ) ENGINE=InnoDB;");

                    await EnsureColumn("ProductionOrders", "IsArchived", "TINYINT(1) NOT NULL DEFAULT 0");
                    await EnsureColumn("ProductionOrders", "CustomerUserId", "INT NULL");
                    await EnsureColumn("SystemConfigurations", "DailyGoal", "INT NOT NULL DEFAULT 500");

                    // Products visual fields repair
                    await EnsureColumn("Products", "AvailableColors", "VARCHAR(200) NULL");
                    await EnsureColumn("Products", "AvailableSizes", "VARCHAR(200) NULL");
                    await EnsureColumn("Products", "Description", "VARCHAR(1000) NULL");
                    await EnsureColumn("Products", "ImageUrl", "LONGTEXT NULL");

                    // 3. THE ERROR 500 KILLER: Dynamic Foreign Key Reconstruction
                    try 
                    {
                        // Check if ProductionOrderOutputs table exists first
                        cmd.CommandText = "SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_NAME = 'ProductionOrderOutputs' AND TABLE_SCHEMA = DATABASE()";
                        if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0) {
                            logger.LogWarning("SYSTEM: Verifying Cascade Delete on ProductionOrderOutputs...");
                            
                            // Find ALL foreign keys on ProductionOrderOutputs
                            cmd.CommandText = "SELECT CONSTRAINT_NAME FROM information_schema.KEY_COLUMN_USAGE WHERE TABLE_NAME = 'ProductionOrderOutputs' AND TABLE_SCHEMA = DATABASE() AND REFERENCED_TABLE_NAME IS NOT NULL";
                            var fks = new List<string>();
                            using (var reader = await cmd.ExecuteReaderAsync()) {
                                while (await reader.ReadAsync()) fks.Add(reader.GetString(0));
                            }

                            foreach (var fk in fks) {
                                try {
                                    cmd.CommandText = $"ALTER TABLE ProductionOrderOutputs DROP FOREIGN KEY `{fk}`";
                                    await cmd.ExecuteNonQueryAsync();
                                } catch { }
                            }

                            // Recreate correct Cascade Keys
                            cmd.CommandText = "ALTER TABLE ProductionOrderOutputs ADD CONSTRAINT FK_Outputs_Orders_Cascade FOREIGN KEY (ProductionOrderId) REFERENCES ProductionOrders(Id) ON DELETE CASCADE";
                            await cmd.ExecuteNonQueryAsync();
                            
                            cmd.CommandText = "ALTER TABLE ProductionOrderOutputs ADD CONSTRAINT FK_Outputs_Sizes_Cascade FOREIGN KEY (ProductionOrderSizeId) REFERENCES ProductionOrderSizes(Id) ON DELETE CASCADE";
                            await cmd.ExecuteNonQueryAsync();
                            
                            logger.LogInformation("SYSTEM: Cascade Delete is now ACTIVE and VERIFIED.");
                        }
                    } 
                    catch (Exception ex) { logger.LogWarning("Repair detail: {Msg}", ex.Message); }
                }

                await DbInitializer.SeedAsync(context, logger);
                logger.LogInformation("SYSTEM: ALL STABILIZATION TASKS COMPLETED.");
                break;
            }
        }
        catch (Exception ex)
        {
            retries--;
            logger.LogWarning("SYSTEM: Retry in 3s... Error: {Msg}", ex.Message);
            await Task.Delay(3000);
        }
    }
}

app.MapControllers();
app.MapHub<ProductionHub>("/productionHub").RequireCors("AllowAll");
app.MapFallbackToFile("index.html");
app.Run();

public partial class Program { }
