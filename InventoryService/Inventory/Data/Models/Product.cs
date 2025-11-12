using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Data;

/// <summary>
/// Product Entity represents the physical goods in the inventory system. This act like a category for product profiles.
/// </summary>
[Table("Products")]
public partial class ProductEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// The name of the product. Can be in Khmer, English, or Mixed.
    /// </summary>
    [Required]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Mixed Language processed Name. Easier searching and duplicate checking
    /// </summary>
    public string NormalizedName { get; set; } = null!;

    /// <summary>
    /// English only processed Name. Easier searching and duplicate checking
    /// </summary>
    public string EnglishNormalizedName { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>
    /// Total stock across all Profiles
    /// </summary>
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