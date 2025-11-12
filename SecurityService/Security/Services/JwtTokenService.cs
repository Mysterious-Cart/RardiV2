using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Security.Data;

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

    private IConfiguration JwtSettings { get; }
    private char[] PrivateKey { get; }
    private char[] PublicKey { get; }
    private JwtTokenServiceOption _options { get; }
    public JwtTokenService(IConfiguration configuration, Action<JwtTokenServiceOption>? configure = null)
    {
        _configuration = configuration;
        _tokenHandler = new JwtSecurityTokenHandler();
        _options = new JwtTokenServiceOption();
        configure?.Invoke(_options);

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

    public string GenerateToken(User user, IList<string> roles)
    {
        using var _rsa = RSA.Create();
    
        if(_options.IsKeyEncrypted && _options.KeyEncryptionPassword != null)
            _rsa.ImportFromEncryptedPem(PrivateKey, _options.KeyEncryptionPassword.ToCharArray());
        else if(!_options.IsKeyEncrypted)
            _rsa.ImportFromPem(PrivateKey);
        else
            throw new ArgumentException("Key is encrypted but no password provided");
        var rsaParams = _rsa.ExportParameters(true);
        
        var key = new RsaSecurityKey(rsaParams)
        {
            KeyId = GenerateKeyId(PublicKey)
        };
    
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
        if(roles is not null && roles.Count > 0)
        {
            foreach (var role in roles) 
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
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
        
            if(_options.IsKeyEncrypted && _options.KeyEncryptionPassword != null)
                _rsa.ImportFromEncryptedPem(PublicKey, _options.KeyEncryptionPassword.ToCharArray());
            else if(!_options.IsKeyEncrypted)
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
    
    public async Task GenerateRefreshToken(User user, IAuthenticationService authenticationService)
    {
        // Implementation for refresh token generation can be added here
        await Task.CompletedTask;
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
            if (_options.IsKeyEncrypted && _options.KeyEncryptionPassword != null)
                rsa.ImportFromEncryptedPem(PublicKey, _options.KeyEncryptionPassword.ToCharArray());
            else if (!_options.IsKeyEncrypted)
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