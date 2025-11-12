namespace Customer.Assets.Domain;

public record Cart(Guid Id, Guid CustomerId, Guid VehicleId, CartStatus Status, List<CartItem> Items, Location Location);
public record CartItem(Guid Id, string Description, decimal Price, int Quantity, decimal Total);