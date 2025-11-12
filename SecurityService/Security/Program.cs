
using Microsoft.EntityFrameworkCore;
using Security;
using Security.Service;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Security.Data;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon.RDS;
using Security.Assets.ServiceConfig;
using Mapster;
using Amazon;
using Newtonsoft.Json.Linq;
using Amazon.RDS.Model;
var builder = WebApplication.CreateBuilder(args);

TypeAdapterConfig.GlobalSettings.EnableJsonMapping();
/*

#region Retrieve Secrets from AWS Secrets Manager

string ConnectionString = string.Empty;
RSAKeyPair rsaKeyPair = new(string.Empty, string.Empty);
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
#endregion
*/
try
{
    builder.Services.AddDbContextFactory<SecurityContext>(options =>
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

var rsakeypairs = new RSAKeyPair(builder.Configuration["JwtSettings:PublicKey"]!, builder.Configuration["JwtSettings:PrivateKey"]!);
builder.Services.AddScoped<ISecurityService,SecurityService>();
builder.Services.AddScoped<SeederService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<LocationService>();
builder.Services.AddIdentityConfiguration();
builder.Services.AddSecurityConfiguration(builder.Configuration, rsakeypairs);
builder.Services.AddHotChocolateConfiguration(builder);


builder.Services.AddControllers();
var app = builder.Build();

// Apply migrations with error handling
try
{
    var context = app.Services.GetRequiredService<IDbContextFactory<SecurityContext>>().CreateDbContext();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Applying database migrations...");
    //context.Database.Migrate();
    logger.LogInformation("Database migrations applied successfully.");

    // Seed initial data
    var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<SeederService>();
    await seeder.Seed();
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
