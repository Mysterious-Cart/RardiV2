using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Rardi.Shared.Services;
using Rardi.Shared.Assets;
using Rardi.Web.Client.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MudBlazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add device-specific services used by the Rardi.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddMudServices();
builder.Services.AddScoped<AppService>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddRardiClient();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddRardiAuthentication();
await builder.Build().RunAsync();
