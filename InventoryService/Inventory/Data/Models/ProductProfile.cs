using Inventory.Data;

/// <summary>
/// Product profile represents specific details about a product variant, including its quantity, price, SKU, and location.
/// This allows different profile for each Product, for different locations of warehouses, etc.
/// </summary>
public class ProductProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProductId { get; set; }
    public ProductEntity? Product { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string SKU { get; set; } = null!;

    public Guid LocationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public bool isDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}