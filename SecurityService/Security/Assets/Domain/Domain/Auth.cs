using Security.Data;
using Security.Assets;
namespace Security.Assets;
public abstract record PayloadBase
{
    public bool Success { get; }
    public string Message { get; }

    protected PayloadBase(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}

/// <summary>
/// Authentication response payload
/// </summary>
public record AuthResult(bool Success, User? User, string Message) : PayloadBase(Success, Message);
public record RoleResult(bool Success, Role? Role, string Message) : PayloadBase(Success, Message);
public record UserResult(bool Success, User? User, string Message) : PayloadBase(Success, Message);
public record LoginResult(bool Success, User? User, string? Token,string? RefreshToken, string Message) : PayloadBase(Success, Message);
public record UserRegistrationResult(bool Succeeded, User? User, string Message) : PayloadBase(Succeeded, Message);
public record RefreshTokenRenewResult(bool Succeeded, string? Token, string? RefreshToken);