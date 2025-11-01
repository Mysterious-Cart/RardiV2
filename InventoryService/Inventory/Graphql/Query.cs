namespace Inventory.Graphql;

using System.Security.Claims;
using HotChocolate.Authorization;
using DTO = Inventory.Assets.Domain;
using Inventory.Services;
using Inventory.Graphql.DataLoader;

public class Query
{

    [UsePaging]
    [UseSorting]
    public async Task<IEnumerable<DTO.Product>> GetProduct([Service] InventoryManagementService inventoryManagementService, CancellationToken cancellationToken, string SearchByName = "")
    {
        return await inventoryManagementService.GetAllProducts(cancellationToken, SearchByName);
    }

    public async Task<DTO.Product> GetProductById(ProductLoader productLoader, Guid id, CancellationToken cancellationToken)
    {
        return await productLoader.LoadRequiredAsync(id, cancellationToken);
    }
    
    public async Task<IEnumerable<DTO.Order>> GetOrders()
    {
        return new List<DTO.Order>();
    }
}