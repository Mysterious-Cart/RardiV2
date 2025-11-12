namespace Inventory.Data;

public partial class Supplier
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<ProductEntity> Products { get; set; } = [];
    public bool isDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    
}