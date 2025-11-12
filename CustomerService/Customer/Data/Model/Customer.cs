using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Customer.Data;

[PrimaryKey("Id")]
public partial class CustomerProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = null!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public ICollection<Guid> Locations { get; set; } = [];
    public ICollection<Vehicle> Vehicles { get; set; } = [];
    
    public CustomerProfile()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }
}
