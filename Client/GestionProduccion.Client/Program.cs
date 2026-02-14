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

builder.Services.AddTransient<AuthHeaderHandler>();

// Configure HttpClient with JSON options
builder.Services.AddScoped(sp => 
{
    var handler = sp.GetRequiredService<AuthHeaderHandler>();
    handler.InnerHandler = new HttpClientHandler();
    
    var client = new HttpClient(handler)
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    };
    return client;
});

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddSingleton<SignalRService>();
builder.Services.AddSingleton<ToastService>();
builder.Services.AddScoped<UserStateService>();

await builder.Build().RunAsync();
