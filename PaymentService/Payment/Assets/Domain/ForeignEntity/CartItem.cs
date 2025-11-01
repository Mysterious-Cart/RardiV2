using System;
using HotChocolate.ApolloFederation.Types;

namespace Payment.Assets.Domain.ForeignEntity;

/// <summary>
/// Represents an item in a shopping cart in the foreign entity domain.
/// Customer Service will manage cart items and resolve this Entity.
/// </summary>
[ExtendServiceType]
public class CartItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
