using GestionProduccion.Data;
using Microsoft.EntityFrameworkCore;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Services;
using GestionProduccion.Hubs;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// --- 1. DATABASE CONFIGURATION ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)))
);

// --- 2. DEPENDENCY INJECTION (Armored) ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IProductionOrderService, ProductionOrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IReportService, ReportService>();
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// --- 4. CONTROLLERS & JSON REPAIR ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Fix for circular references (Architect Rule 7)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // Fix for Enums as strings (Architect Rule 9)
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // Maintain PascalCase for consistency but allow case-insensitivity for incoming data
        options.JsonSerializerOptions.PropertyNamingPolicy = null; 
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        // Safeguard against nulls (Architect Rule 48)
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- 5. CORS REPAIR (Architect Rule 11) ---
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

// --- 6. MIDDLEWARE PIPELINE (Correct Order) ---
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Architect Rule 18
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseWebAssemblyDebugging();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles(); // Architect Rule 19
app.UseStaticFiles();          // Architect Rule 19
app.UseRouting();

app.UseCors("AllowAll"); // Must be before Auth

app.UseAuthentication();
app.UseAuthorization();

// --- 7. AUTOMATIC MIGRATIONS (Architect Rule 5) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogCritical(ex, "FATAL: Database migration failed at startup.");
    }
}

app.MapControllers();
app.MapHub<ProductionHub>("/productionHub");
app.MapFallbackToFile("index.html");

app.Run();
