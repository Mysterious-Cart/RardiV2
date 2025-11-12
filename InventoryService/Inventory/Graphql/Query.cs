namespace Inventory.Graphql;

using System.Security.Claims;
using HotChocolate.Authorization;
using DTO = Inventory.Assets.Domain;
using Inventory.Assets.Interface;
using Inventory.Graphql.DataLoader;

public class Query
{

    [Authorize]
    [UsePaging]
    [UseSorting]
    public async Task<IEnumerable<DTO.Product>> GetProduct(ClaimsPrincipal claimsPrincipal,IInventoryManagementService inventoryManagementService, CancellationToken cancellationToken, string SearchByName = "")
    {
        return await inventoryManagementService.GetAllProducts(Guid.Parse(claimsPrincipal.Claims.First(i => i.Type == "location").Value), SearchByName, false, cancellationToken);
    }

    public async Task<DTO.Product> GetProductById(ProductLoader productLoader, Guid id, CancellationToken cancellationToken)
    {
        return await productLoader.LoadRequiredAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<DTO.Order>> GetOrders(IOrderManagementService orderManagementService, CancellationToken cancellationToken)
    {
        return [];
    }
}