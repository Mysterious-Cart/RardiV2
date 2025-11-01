using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Data;

/// <summary>
/// Product Entity represents the physical goods in the inventory system. This act like a category for product profiles.
/// </summary>
[Table("Products")]
public partial class ProductEntity
{
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    // Easier searching and duplicate checking
    public string NormalizedName { get; set; } = null!;

    public string Description { get; set; } = null!;

    // Total stock across all Profiles
    public int TotalStock { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool isDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public ICollection<Supplier> Suppliers { get; set; } = [];
    public ICollection<ProductSupplierProfiles> SupplierProfiles { get; set; } = [];
    public ICollection<ProductProfile> Details { get; set; } = [];
    public ICollection<Category> Categories { get; set; } = [];

}