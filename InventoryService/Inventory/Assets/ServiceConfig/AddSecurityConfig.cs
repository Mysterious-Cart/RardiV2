using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
namespace Inventory.Assets.ServiceConfig;

public static class AddSecurityConfig
{
    public static IServiceCollection AddSecurityConfiguration(this IServiceCollection services, IConfiguration configuration, RSAKeyPair rsaKeyPair)
    {
        var _rsa = RSA.Create();
        _rsa.ImportFromPem(rsaKeyPair.PublicKey.ToCharArray());
        var key = new RsaSecurityKey(_rsa);
        services.AddAuthorization()
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddCookie(IdentityConstants.ApplicationScheme)
        .AddCookie(IdentityConstants.ExternalScheme)
        .AddCookie(IdentityConstants.TwoFactorUserIdScheme)
        .AddJwtBearer(options =>
        {

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = configuration["JwtSettings:Issuer"],
                ValidateAudience = false,
                ValidateLifetime = true,
                TryAllIssuerSigningKeys = true,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.FromMinutes(1),
                AuthenticationType = "JWT",

            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {

                    // Check for token in different locations
                    if (string.IsNullOrEmpty(context.Token))
                    {
                        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                        {
                            context.Token = authHeader.Substring("Bearer ".Length).Trim();
                        }
                    }

                    return Task.CompletedTask;
                },

                OnTokenValidated = context =>
                {
                    Console.WriteLine($"Token validated for user: {context.Principal?.Identity?.Name}");
                    return Task.CompletedTask;
                }

            };
        });

        return services;
    }
}
