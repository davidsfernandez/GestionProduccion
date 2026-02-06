using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GestionProduccion.Client;
using Microsoft.AspNetCore.Components.Authorization;
using GestionProduccion.Client.Auth;
using GestionProduccion.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTransient<AuthHeaderHandler>();

// Use the current BaseAddress dynamically (will be the same origin as the Blazor app)
builder.Services.AddHttpClient("API", client => 
{
    var baseAddress = builder.HostEnvironment.BaseAddress;
    client.BaseAddress = new Uri(baseAddress);
})
.AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddSingleton<SignalRService>();

await builder.Build().RunAsync();
