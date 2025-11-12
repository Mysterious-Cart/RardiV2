using Customer.Data;
namespace Customer.Data;
public class Cart
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public CustomerVehicleProfile? CustomerVehicleProfile { get; set; } 
    public CartStatus Status { get; set; }
    public List<CartItem> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid LocationId { get; set; }

    public Cart()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Status = CartStatus.InProgress;
    }
}