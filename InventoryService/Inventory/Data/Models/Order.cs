using Inventory.Assets;

namespace Inventory.Data;


/// <summary>
/// Represents a record of product imports from suppliers.
/// </summary>
public class Order
{
    public Guid Id { get; set; }
    public Guid ProductSupplierProfileId { get; set; }
    public ProductSupplierProfiles? ProductProfile { get; set; }
    public OperationStatus Status { get; set; } = OperationStatus.Pending;
    public int Quantity { get; set; }

    // Import Price default to the latest price from Profiles, If changes this become the profiles latest price
    // When order succeeds
    public decimal? ImportPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    
}

