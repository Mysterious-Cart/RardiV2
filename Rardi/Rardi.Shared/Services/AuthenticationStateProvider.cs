using System.Buffers.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using Rardi.Shared.Services;

public class RardiAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly AuthTokenService _authTokenService;
    private readonly HttpClient _httpClient;
    private AuthenticationState? _cachedAuthenticationState;
    public RardiAuthenticationStateProvider(AuthTokenService authTokenService, HttpClient httpClient)
    {
        _authTokenService = authTokenService;
        _httpClient = httpClient;
    }

    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if( _cachedAuthenticationState is not null)
        {
            return _cachedAuthenticationState;
        }
        else
        {
            var token = await _authTokenService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }else{
                await AuthenticateUser(token);
                return _cachedAuthenticationState!;
            }
        }
    }

    public async Task AuthenticateUser(string jwtToken)
    {
        await _authTokenService.SetTokenAsync(jwtToken);
        var ParsedJwtToken = ReadJwtToken(jwtToken);
        if (ParsedJwtToken is null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(jwtToken), "Invalid JWT Token");
        } 

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            ParsedJwtToken!.Claims, "Rardi Authentication"));

        _cachedAuthenticationState = new AuthenticationState(user);
        NotifyAuthenticationStateChanged(Task.FromResult(_cachedAuthenticationState));
    }

    private JwtSecurityToken? ReadJwtToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        try
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(token);
        }
        catch
        {
            return null;
        }
    }
}