using GestionProduccion.Data;
using Microsoft.EntityFrameworkCore;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Services;
using GestionProduccion.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Get Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// 3. Add Custom Services
builder.Services.AddScoped<IOpService, OpService>();

// 4. Add SignalR
builder.Services.AddSignalR();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Map Hubs
app.MapHub<ProducaoHub>("/producaoHub");

app.Run();
