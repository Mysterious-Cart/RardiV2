using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;
using Inventory.Data;
using Inventory.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
try
{
    builder.Services.AddDbContextFactory<InventoryContext>(options =>
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
    
builder.Services.AddScoped<InventoryManagementService>();

builder.Services
    .AddGraphQLServer()
    .AddApolloFederation()
    .AddQueryType<Inventory.Graphql.Query>()
    .AddMutationType<Inventory.Graphql.Mutation>()
    .AddSubscriptionType<Inventory.Graphql.Subscription>()
    .AddPostgresSubscriptions((sp, option) => {
        option.ConnectionFactory = ct =>
        {
            var Npgsqlbuilder = new NpgsqlDataSourceBuilder(  
                builder.Configuration.GetConnectionString("DefaultConnection")
            );

            // we do not need pooling for long running connections
            Npgsqlbuilder.ConnectionStringBuilder.Pooling = false;
            // we set the keep alive to 30 seconds
            Npgsqlbuilder.ConnectionStringBuilder.KeepAlive = 30;
            // as these tasks often run in the background we do not want to enlist them so they do not
            // interfere with the main transaction
            Npgsqlbuilder.ConnectionStringBuilder.Enlist = false;

            var dataSource = Npgsqlbuilder.Build();

            return dataSource.OpenConnectionAsync(ct);
        }
        
        ;
    })
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddAuthorization()
    .InitializeOnStartup()
    .ModifyRequestOptions(
        o => o.IncludeExceptionDetails =
            builder.Environment.IsDevelopment());



builder.Services.AddControllers();
var app = builder.Build();

// Apply migrations with error handling
try
{
    var context = app.Services.GetRequiredService<IDbContextFactory<InventoryContext>>().CreateDbContext();
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
app.UseWebSockets();
app.MapControllers();

app.Run();
