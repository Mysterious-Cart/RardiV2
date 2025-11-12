namespace Inventory.Assets.Interface;

using Assets.Domain;
using Inventory.Data;

public interface IInventoryManagementService
{
    Task<Product> GetProductById(Guid productId);
    Task<Product> CreateProduct(CreateProduct createProduct, Guid LocationId);
    Task<IEnumerable<Product>> GetAllProducts(Guid LocationId, string? SearchByName = "", bool IncludeDeleted = false, CancellationToken cancellationToken = default);
    Task<Product> UpdateProductStock(Guid productId, int quantityChange);
    Task<Guid> DeleteProduct(Guid productId);
     Task<Guid> SoftDeleteProduct(Guid productId);
    Task<Product> RestoreProduct(Guid productId);

}