using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Security.Data;

public class RefreshToken
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// The refresh token value (hashed for security).
    /// </summary>
    [Required]
    public string Token { get; set; } = null!;
    
    /// <summary>
    /// User who owns this refresh token.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Expiration timestamp (typically 7-30 days from creation).
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Creation timestamp for audit trail.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// IP address from which token was created (security audit).
    /// </summary>
    public string? CreatedByIp { get; set; }
    
    /// <summary>
    /// Navigation property to User.
    /// </summary>
    public User User { get; set; } = null!;
    
    /// <summary>
    /// Checks if refresh token is still valid.
    /// </summary>
    public bool IsValid => ExpiresAt > DateTime.UtcNow;
}