using HotChocolate.ApolloFederation.Types;
using Payment.Assets.Domain.ForeignEntity;

namespace Payment.Asset.ForeignEntity;

/// <summary>
/// Represents a shopping cart in the foreign entity domain.
/// Customer Service will manage carts and resolve this Entity.
/// </summary>
[ExtendServiceType]
public class Cart
{
    public Guid Id { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid CustomerId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CartItem> Items { get; set; } = [];
}