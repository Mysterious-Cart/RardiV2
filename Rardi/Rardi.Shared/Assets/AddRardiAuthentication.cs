using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Rardi.Shared.Services;

public static class AddAuthentication
{
    public static IServiceCollection AddRardiAuthentication(this IServiceCollection services)
    {
        services.AddAuthorizationCore();
        services.AddCascadingAuthenticationState();
        services.AddScoped<AuthTokenService>();
        services.AddSingleton<AuthenticationStateProvider, RardiAuthenticationStateProvider>();
        return services;
    }
}