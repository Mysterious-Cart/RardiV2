namespace Inventory.Assets.ServiceConfig;

public record RSAKeyPair(string PublicKey, string PrivateKey);
public class DatabaseConfig
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string? Database { get; set; }

    public DatabaseConfig(string username, string password, string? database = null)
    {
        Username = username;
        Password = password;
        Database = database;
    }
}