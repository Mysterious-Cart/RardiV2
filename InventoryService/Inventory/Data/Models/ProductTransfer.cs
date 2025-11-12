namespace Inventory.Data;

using Assets;

/// <summary>
/// Represents a transfer of products between locations.
/// </summary>
public class ProductTransfer
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public ProductEntity? Product { get; set; }

    public Guid FromLocationId { get; set; }
    public Location? FromLocation { get; set; }

    public Guid ToLocationId { get; set; }
    public Location? ToLocation { get; set; }

    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public OperationStatus Status { get; set; } = OperationStatus.Pending;
}