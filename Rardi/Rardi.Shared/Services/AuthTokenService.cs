using System.IdentityModel.Tokens.Jwt;

namespace Rardi.Shared.Services;

/// <summary>
/// Manages JWT tokens for GraphQL client authentication.
/// Stores tokens in localStorage and provides refresh functionality.
/// </summary>
public class AuthTokenService()
{
    private const string TOKEN_KEY = "rardi_access_token";
    private const string REFRESH_TOKEN_KEY = "rardi_refresh_token";
    private string? _cachedToken;

    /// <summary>
    /// Gets the current access token from cache or storage.
    /// </summary>
    public async Task<string?> GetTokenAsync()
    {
        if (!string.IsNullOrEmpty(_cachedToken) && !IsTokenExpired(_cachedToken))
        {
            return _cachedToken;
        }

        // Try to get from localStorage (implement based on your storage strategy)
        _cachedToken = await GetStoredTokenAsync();
        
        if (!string.IsNullOrEmpty(_cachedToken) && IsTokenExpired(_cachedToken))
        {
            // Token expired, try to refresh
            await RefreshTokenAsync();
        }

        return _cachedToken;
    }

    /// <summary>
    /// Stores JWT token after successful authentication.
    /// </summary>
    public async Task SetTokenAsync(string token, string? refreshToken = null)
    {
        _cachedToken = token;
        await StoreTokenAsync(TOKEN_KEY, token);
        
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await StoreTokenAsync(REFRESH_TOKEN_KEY, refreshToken);
        }
    }

    /// <summary>
    /// Clears stored tokens (logout).
    /// </summary>
    public async Task ClearTokensAsync()
    {
        _cachedToken = null;
        await RemoveTokenAsync(TOKEN_KEY);
        await RemoveTokenAsync(REFRESH_TOKEN_KEY);
    }

    /// <summary>
    /// Checks if token is expired or about to expire (within 5 minutes).
    /// </summary>
    private bool IsTokenExpired(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var expiryTime = jwtToken.ValidTo;
            
            // Consider token expired if it expires within 5 minutes
            return expiryTime <= DateTime.UtcNow.AddMinutes(5);
        }
        catch
        {
            return true; // Invalid token
        }
    }

    /// <summary>
    /// Refreshes access token using refresh token.
    /// </summary>
    private async Task RefreshTokenAsync()
    {
        var refreshToken = await GetStoredTokenAsync(REFRESH_TOKEN_KEY);
        if (string.IsNullOrEmpty(refreshToken))
        {
            await ClearTokensAsync();
            return;
        }

        // TODO: Call SecurityService refresh endpoint
        // For now, clear tokens to force re-login
        await ClearTokensAsync();
    }

    // Platform-specific storage implementations
    private Task<string?> GetStoredTokenAsync(string key = TOKEN_KEY)
    {
        // Implement based on platform (localStorage for Web, SecureStorage for MAUI)
        return Task.FromResult<string?>(null);
    }

    private Task StoreTokenAsync(string key, string value)
    {
        // Implement based on platform
        return Task.CompletedTask;
    }

    private Task RemoveTokenAsync(string key)
    {
        // Implement based on platform
        return Task.CompletedTask;
    }
}