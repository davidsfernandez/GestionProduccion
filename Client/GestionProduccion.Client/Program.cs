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

builder.Services.AddHttpClient("API", client => client.BaseAddress = new Uri("http://localhost:5151"))
    .AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));


builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddSingleton<SignalRService>();

await builder.Build().RunAsync();
