namespace Inventory.Graphql;

using DTO = Inventory.Assets.Domain;
using Inventory.Services;

public class Mutation
{
    public async Task<DTO.Product> CreateProduct(InventoryManagementService service, DTO.CreateProduct input)
    {
        try
        {
            return await service.CreateProduct(input);
        }
        catch (Exception ex)
        {
            throw new GraphQLException(new Error(ex.Message, "PRODUCT_CREATION_ERROR"));
        }
    }

    [GraphQLDescription("Delete product by given ID. Return the deleted product Id")]

    public async Task<Guid> DeleteProduct(InventoryManagementService service, Guid ProductId)
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

    public async Task CreateOrder()
    {

    }

    public async Task CancelOrder()
    {

    }
    
}