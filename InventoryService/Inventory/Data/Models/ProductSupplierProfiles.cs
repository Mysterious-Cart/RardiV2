namespace Inventory.Data;

public class ProductSupplierProfiles
{
    public Guid ProductId { get; set; }
    public ProductEntity? Product { get; set; }

    public Guid SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    // Latest price from supplier for this product
    public decimal LatestImportPrice { get; set; }

    public bool isDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}