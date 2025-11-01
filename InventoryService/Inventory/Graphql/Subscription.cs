namespace Inventory.Graphql;

using Inventory.Assets.Domain;
using Inventory.Services;
public class Subscription
{
    [Subscribe]
    public Guid OnProductCreated([EventMessage] Product product) => product.Id;
    [Subscribe]
    public Guid OnProductDeleted([EventMessage] Guid productId) => productId;
    [Subscribe]
    public Product OnProductUpdated([EventMessage] Product product) => product;

    [Subscribe]
    public Order OnOrderAdded([EventMessage] Order order) => order;
    [Subscribe]
    public Order OnOrderCancelled([EventMessage] Order order) => order;

}