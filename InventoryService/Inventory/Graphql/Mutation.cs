namespace Inventory.Graphql;

using DTO = Inventory.Assets.Domain;
using Inventory.Assets.Interface;
using System.Security.Claims;
using HotChocolate.Authorization;

public class Mutation
{
    [Authorize]
    public async Task<DTO.Product> CreateProduct(ClaimsPrincipal claimsPrincipal,IInventoryManagementService service, DTO.CreateProduct input)
    {
        try
        {
            Console.WriteLine("Location ID: " + claimsPrincipal.Claims.First(i => i.Type == "location").Value);
            return await service.CreateProduct(input, Guid.Parse(claimsPrincipal.Claims.First(i => i.Type == "location").Value));
        }
        catch (Exception ex)
        {
            throw new GraphQLException(new Error(ex.Message, "PRODUCT_CREATION_ERROR"));
        }
    }

    [GraphQLDescription("Delete product by given ID. Return the deleted product Id")]
    [Authorize]
    public async Task<Guid> DeleteProduct(IInventoryManagementService service, Guid ProductId)
    {
        try
        {
            return await service.SoftDeleteProduct(ProductId);
        }
        catch (Exception ex)
        {
            throw new GraphQLException(new Error(ex.Message, "PRODUCT_DELETION_ERROR"));

        }
    }

    public async Task CreateOrder(IOrderManagementService orderManagementService, Guid ProductSupplierProfileId, int Quantity, decimal? ImportPrice)
    {
        await orderManagementService.CreateOrder(ProductSupplierProfileId, Quantity, ImportPrice);
    }

    public async Task CancelOrder(IOrderManagementService orderManagementService, Guid OrderId)
    {
        await orderManagementService.CompleteOrder(OrderId);
    }
    
}