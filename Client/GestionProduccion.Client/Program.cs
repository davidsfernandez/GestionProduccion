using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GestionProduccion.Client;
using Microsoft.AspNetCore.Components.Authorization;
using GestionProduccion.Client.Auth;
using GestionProduccion.Client.Services;
using System.Text.Json.Serialization;
using System.Text.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// --- 1. HTTP CLIENT CONFIGURATION (Dynamic BaseAddress) ---
builder.Services.AddTransient<AuthHeaderHandler>();

builder.Services.AddScoped(sp => 
{
    var handler = sp.GetRequiredService<AuthHeaderHandler>();
    handler.InnerHandler = new HttpClientHandler();
    
    var client = new HttpClient(handler)
    {
        // Architect Rule 24: Dynamic assignment
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    };
    return client;
});

// --- 2. AUTHENTICATION (Armored) ---
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// --- 3. FRONTEND SERVICES (Scoped) ---
builder.Services.AddSingleton<SignalRService>();
builder.Services.AddSingleton<ToastService>();
builder.Services.AddScoped<UserStateService>();

await builder.Build().RunAsync();
