using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Security.Data;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore;

namespace Security.Service;

// # NEED TO IMPLEMENT ROLE CHANGES TOKEN OUTDATE.

/// <summary>
/// Service for generating JWT tokens for inter-service communication, across microservices.
/// This is used to authenticate requests between services in a microservices architecture.
/// </summary>
public class JwtTokenService
{

    private readonly IConfiguration _configuration;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly SecurityContext _securityContext;

    private JwtTokenServiceOption Options { get; }
    private IConfiguration? JwtSettings { get; }
    private char[]? PrivateKey { get; }
    private char[]? PublicKey { get; }
    public JwtTokenService(IDbContextFactory<SecurityContext> securityContextFactory, IConfiguration configuration, Action<JwtTokenServiceOption>? configure = null)
    {
        _securityContext = securityContextFactory.CreateDbContext();
        _configuration = configuration;
        _tokenHandler = new JwtSecurityTokenHandler();
        Options = new JwtTokenServiceOption();
        configure?.Invoke(Options);

        // DEBUG MODE; USE CONFIGURED KEYS
#if DEBUG

        // Load keys from configuration
        JwtSettings = _configuration.GetSection("JwtSettings") ?? throw new ArgumentException("JwtSettings not found in configuration");
        PrivateKey = (JwtSettings["PrivateKey"] ?? throw new ArgumentException("PrivateKey not found in JwtSettings")).ToCharArray();
        PublicKey = (JwtSettings["PublicKey"] ?? throw new ArgumentException("PublicKey not found in JwtSettings")).ToCharArray();
        
#endif

#if !DEBUG
        throw new NotImplementedException("JWT authentication is not implemented in non-debug mode.");
#endif
    }

    public async Task<string> GenerateToken(string RefreshToken){
        var token = _securityContext.RefreshTokens.Select(i => new{i.Token, i.UserId}).FirstOrDefault(rt => rt.Token == RefreshToken)
        ??throw new ArgumentException("Invalid refresh token");
        
        var userFound = await _securityContext.Users.FindAsync(token.UserId) 
            ??throw new ArgumentException("User not found for the given refresh token");

        var roles = _securityContext.Users.Where(i => i.Id == userFound.Id).Select(i => i.Roles).FirstOrDefault()?.ToList()??[];
        return await GenerateToken(userFound.Id, roles);
    }
    public async Task<string> GenerateToken(Guid userId, IList<Role>? roles)
    {
        var user = await _securityContext.Users.FindAsync(userId)
            ?? throw new ArgumentException("User not found for the given user ID");
        #if DEBUG
        using var _rsa = RSA.Create();

        if (Options.IsKeyEncrypted && Options.KeyEncryptionPassword != null)
            _rsa.ImportFromEncryptedPem(PrivateKey, Options.KeyEncryptionPassword.ToCharArray());
        else if (!Options.IsKeyEncrypted)
            _rsa.ImportFromPem(PrivateKey);
        else
            throw new ArgumentException("Key is encrypted but no password provided");
        var rsaParams = _rsa.ExportParameters(true);

        var key = new RsaSecurityKey(rsaParams)
        {
            KeyId = GenerateKeyId(PublicKey ?? throw new ArgumentException("Public key not found"))
        };

        #endif

        #if !DEBUG
             var key = new RsaSecurityKey(rsaParams)
            {
                KeyId = GenerateKeyId(PublicKey ?? throw new ArgumentException("Public key not found"))
            };
        #endif

        var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
        var now = DateTime.UtcNow;
        var unixTimeSecondsForNow = new DateTimeOffset(now).ToUnixTimeSeconds();
        var unixTimeSecondsForExp =
            new DateTimeOffset(now.AddMinutes(int.Parse(JwtSettings["ExpirationInMinutes"]!))).ToUnixTimeSeconds();

        // STANDARD PURPOSES FOR CAPABILITES (STORING JWT INFO FOR BOTH CLAIM AND TOKEN)

        var claims = new List<Claim>
        {
            // Username
            new(JwtRegisteredClaimNames.Name, user.UserName!),
            // User ID
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            // Issued At (UnixTime for Jwt standard for claim)
            new(JwtRegisteredClaimNames.Iat, unixTimeSecondsForNow.ToString(), ClaimValueTypes.Integer64),
            // Not Before (UnixTime for Jwt standard for claim)
            new(JwtRegisteredClaimNames.Nbf, unixTimeSecondsForNow.ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Iss, JwtSettings["Issuer"]!),
            new(JwtRegisteredClaimNames.Aud, JwtSettings["Audience"]!),
            new(JwtRegisteredClaimNames.Exp, unixTimeSecondsForExp.ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("location", user.LocationId.ToString(), ClaimValueTypes.String),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName!),

        };
        if (roles is not null && roles.Count > 0)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Id.ToString()!));
            }
        }


        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = now.AddMinutes(int.Parse(JwtSettings["ExpirationInMinutes"]!)),
            NotBefore = now,
            IssuedAt = now,
            Issuer = JwtSettings["Issuer"],
            Audience = JwtSettings["Audience"],
            SigningCredentials = credentials,

        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = _tokenHandler.WriteToken(token);

        return tokenString;
    }   

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            using var _rsa = RSA.Create();
        
            if(Options.IsKeyEncrypted && Options.KeyEncryptionPassword != null)
                _rsa.ImportFromEncryptedPem(PublicKey, Options.KeyEncryptionPassword.ToCharArray());
            else if(!Options.IsKeyEncrypted)
                _rsa.ImportFromPem(PublicKey); 
            else
                throw new ArgumentException("Key is encrypted but no password provided");
            
            var key = new RsaSecurityKey(_rsa)
            {
                KeyId = GenerateKeyId(PublicKey)
            };

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = JwtSettings["Issuer"],
                ValidAudience = JwtSettings["Audience"],
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            var principal = _tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Token validation failed: {ex.Message}");
            return null;
        }
    }

    public async Task<string> GenerateRefreshToken(Guid userId)
    {
        // Implementation for refresh token generation can be added here
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var refreshToken = Convert.ToBase64String(randomBytes);

        _securityContext.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("JwtSettings:RefreshTokenExpiration"))
        });

        await _securityContext.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<bool> ValidateRefreshToken(Guid userId, string refreshToken)
    {
        var token = await _securityContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.Token == refreshToken);

        if (token == null || !token.IsValid)
        {
            return false;
        }

        return true;
    }

    public async Task<string> RotateRefreshToken(Guid userId, string oldRefreshToken)
    {
        var token = await _securityContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.Token == oldRefreshToken);

        if (token == null || !token.IsValid)
        {
            throw new InvalidOperationException("Invalid refresh token.");
        }

        // Invalidate the old token
        token.ExpiresAt = DateTime.UtcNow;
        _securityContext.RefreshTokens.Update(token);

        // Generate a new refresh token
        var newRefreshToken = await GenerateRefreshToken(userId);

        await _securityContext.SaveChangesAsync();

        return newRefreshToken;
    }

    public async Task RenewRefreshToken(Guid userId, string refreshToken)
    {
        var token = await _securityContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.Token == refreshToken);

        if (token != null)
        {
            _securityContext.RefreshTokens.Remove(token);
            await _securityContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Generate JWKS (JSON Web Key Set) for JWT validation by router
    /// </summary>
    /// <returns>JWKS object containing the public key information</returns>
    public object GenerateJWKS()
    {
        try
        {
            using var rsa = RSA.Create();
            if (Options.IsKeyEncrypted && Options.KeyEncryptionPassword != null)
                rsa.ImportFromEncryptedPem(PublicKey, Options.KeyEncryptionPassword.ToCharArray());
            else if (!Options.IsKeyEncrypted)
                rsa.ImportFromPem(PublicKey);
            else
                throw new ArgumentException("Key is encrypted but no password provided");

            var param = rsa.ExportParameters(false);

            if (param.Modulus == null || param.Exponent == null)
            {
                throw new InvalidOperationException("Invalid RSA public key parameters");
            }

            var jwk = new
            {
                kty = "RSA",
                alg = "RS256",
                kid = GenerateKeyId(PublicKey),
                use = "sig",
                n = Base64UrlTextEncoder.Encode(param.Modulus),
                e = Base64UrlTextEncoder.Encode(param.Exponent)
            };

            var jwks = new { keys = new[] { jwk } };
            Console.WriteLine($"✅ Generated JWKS with kid: {jwk.kid}");

            return jwks;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ JWKS generation failed: {ex.Message}");
            throw new InvalidOperationException($"Failed to generate JWKS: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Generate key ID using RSA key thumbprint (standard approach)
    /// </summary>
    /// <param name="publicKeyPem">The RSA public key</param>
    /// <returns>A consistent key ID</returns>
    private string GenerateKeyId(char[] publicKeyPem)
    {
        using var sha256 = SHA256.Create();
        var publicKeyString = new string(publicKeyPem);
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(publicKeyString));
        return Convert.ToHexString(hash)[..16].ToLowerInvariant();
    }
}

public class JwtTokenServiceOption
{
    private bool _isKeyEncrypted = false;
    private string? _keyEncryptionPassword = null;

    public JwtTokenServiceOption SetKeyEncryption(bool isEncrypted, string? password = null)
    {
        _isKeyEncrypted = isEncrypted;
        _keyEncryptionPassword = password;
        return this;
    }
    public bool IsKeyEncrypted => _isKeyEncrypted;
    public string? KeyEncryptionPassword => _keyEncryptionPassword;
}