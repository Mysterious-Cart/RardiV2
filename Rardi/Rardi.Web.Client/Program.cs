using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Rardi.Shared.Services;
using Rardi.Web.Client.Services;
using MudBlazor.Services;
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add device-specific services used by the Rardi.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddMudServices();
builder.Services.AddScoped<AppService>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddRardiClientWeb()
.ConfigureHttpClient(client => client.BaseAddress = new Uri("http://localhost:5033/graphql/"));


await builder.Build().RunAsync();
