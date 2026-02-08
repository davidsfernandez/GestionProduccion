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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Get Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// 3. Add HttpContextAccessor for accessing current user in services
builder.Services.AddHttpContextAccessor();

// 4. Add Custom Services
builder.Services.AddScoped<IProductionOrderService, ProductionOrderService>();
builder.Services.AddScoped<IUserService, UserService>();

// 5. Add SignalR
builder.Services.AddSignalR();

// 5. Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"] ?? "ClaveSecretaPorDefectoParaDesarrollo12345678";
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

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// 6. Configure Swagger to use JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GestionProduccion API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. <br/> 
                      Enter 'Bearer' [space] and then your token in the text input below. <br/>
                      Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// 7. Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorApp", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
    app.UseWebAssemblyDebugging();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("BlazorApp"); // Add CORS middleware

app.UseAuthentication(); // This must come before UseAuthorization
app.UseAuthorization();

app.MapControllers();

// Map Hubs
app.MapHub<ProductionHub>("/productionHub");

// Fallback to serve the Blazor app from the client project
app.MapFallbackToFile("index.html");

app.Run();
