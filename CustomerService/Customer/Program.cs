using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;
using Customer.Data;

var builder = WebApplication.CreateBuilder(args);
try
{
    builder.Services.AddDbContextFactory<CustomerContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    );
}catch
{
    Console.WriteLine($"Fail to connect to database.");
    throw;
}

builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});


var _rsa = RSA.Create();
_rsa.ImportFromPem(builder.Configuration["JwtSettings:PublicKey"]!.ToCharArray());
var key = new RsaSecurityKey(_rsa);

builder.Services
    .AddAuthorization()
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
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
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
                Console.WriteLine($"JWT Message Received:");
                Console.WriteLine($"  Authorization Header: {context.Request.Headers["Authorization"]}");

                // Check for token in different locations
                if (string.IsNullOrEmpty(context.Token))
                {
                    var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                    {
                        context.Token = authHeader.Substring("Bearer ".Length).Trim();
                        Console.WriteLine($"  Token extracted from Authorization header");
                    }
                }

                return Task.CompletedTask;
            },
            
            OnAuthenticationFailed = context =>
            {
                // When authentication fails forward to API to show request token expiration or failure
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            
            OnTokenValidated = context =>
            {
                Console.WriteLine($"Token validated for user: {context.Principal?.Identity?.Name}");
                return Task.CompletedTask;
            }
            
        };
    });


builder.Services
    .AddGraphQLServer()
    .AddApolloFederation()
    .AddQueryType<Customer.Graphql.Query>()
    .AddAuthorization()
    .InitializeOnStartup()
    .ModifyRequestOptions(
        o => o.IncludeExceptionDetails =
            builder.Environment.IsDevelopment());;

builder.Services.AddControllers();
var app = builder.Build();

// Apply migrations with error handling
try
{
    var context = app.Services.GetRequiredService<IDbContextFactory<CustomerContext>>().CreateDbContext();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Applying database migrations...");
    //context.Database.Migrate();
    logger.LogInformation("Database migrations applied successfully.");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while applying migrations.");
    throw; // Re-throw to prevent startup if migrations fail
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors(i => i
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
);
app.MapGraphQL();

app.MapControllers();

app.Run();
