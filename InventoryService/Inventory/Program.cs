using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Inventory.Data;
using Inventory.Services;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon;
using Inventory.Assets.ServiceConfig;
using Inventory.Assets.Interface;
using Amazon.RDS;
using Amazon.RDS.Model;
using Mapster;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;


var builder = WebApplication.CreateBuilder(args);
TypeAdapterConfig.GlobalSettings.EnableJsonMapping();

#region Retrieve Secrets from AWS Secrets Manager

string ConnectionString = string.Empty;
RSAKeyPair rsaKeyPair = new(string.Empty, string.Empty);
#if !DEBUG
try
{
    const string Database = "Inventory";
    const string DBrequestSecretID = "rds!db-e38144f7-465c-40f0-920f-284cab0bec81";
    const string DBInstanceDescribe = "rardi-inventory-db";
    const string KeyrequestSecretID = "dev/rardi";

    AmazonSecretsManagerClient client = new(RegionEndpoint.APSoutheast1);
    AmazonRDSClient rdsClient = new(RegionEndpoint.APSoutheast1);
    GetSecretValueRequest DBrequest = new() { SecretId = DBrequestSecretID };
    GetSecretValueRequest Keyrequest = new() { SecretId = KeyrequestSecretID };

    var DBSecretKeyResponse = await client.GetSecretValueAsync(DBrequest);
    
    var dbConfig = JObject.Parse(DBSecretKeyResponse.SecretString).Adapt<DatabaseConfig>();
    dbConfig.Database = Database;
    // Get RDS endpoint dynamically
    var rdsResponse = await rdsClient.DescribeDBInstancesAsync(new DescribeDBInstancesRequest
    {
        DBInstanceIdentifier = DBInstanceDescribe

    });

    var endpoint = rdsResponse.DBInstances[0].Endpoint;

    ConnectionString = $"Host={endpoint.Address};Port={endpoint.Port};Database={dbConfig.Database};Username={dbConfig.Username};Password={dbConfig.Password};SSL Mode=Require;";

    var SecreteKeyResponse = await client.GetSecretValueAsync(Keyrequest);
    rsaKeyPair = JObject.Parse(SecreteKeyResponse.SecretString).Adapt<RSAKeyPair>();
}
catch (Exception ex)
{
    Console.WriteLine($"Error retrieving secrets: {ex.Message}");
    throw;
}
#endif
#endregion

#region Configure DbContext
try
{
#if DEBUG
    builder.Services.AddDbContextFactory<InventoryContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    );
#else
    builder.Services.AddDbContextFactory<InventoryContext>(async options =>
    {
        options.UseNpgsql(ConnectionString);
    });
#endif

}
catch
{
    Console.WriteLine($"DB Setup error");
    throw;
}
#endregion


#region Application Service
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

builder.Services.AddScoped<IInventoryManagementService, InventoryManagementService>();
builder.Services.AddScoped<IOrderManagementService, OrderManagementService>();
builder.Services.AddHealthChecks();
#if !DEBUG
builder.Services.AddSecurityConfiguration(builder.Configuration, rsaKeyPair);
#else
builder.Services.AddSecurityConfiguration(builder.Configuration, new RSAKeyPair(
    builder.Configuration["JwtSettings:PublicKey"]!,
    builder.Configuration["JwtSettings:PrivateKey"]!));
#endif
builder.Services.AddHotChocolateConfiguration(builder);
#endregion


builder.Services.AddControllers();
var app = builder.Build();

// Apply migrations with error handling
try
{
    var context = app.Services.GetRequiredService<IDbContextFactory<InventoryContext>>().CreateDbContext();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
    
    if (pendingMigrations.Any())
    {
        logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");
    }
    else
    {
        logger.LogInformation("No pending migrations found");
    }

    logger.LogInformation("Database created successfully.");
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
app.MapHealthChecks("/health");
app.MapGraphQL();
app.UseWebSockets();
app.MapControllers();

app.Run();
