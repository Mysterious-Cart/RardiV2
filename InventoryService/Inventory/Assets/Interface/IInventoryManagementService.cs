namespace Inventory.Assets.Interface;

using Assets.Domain;
using Inventory.Data;

public interface IInventoryManagementService
{
    Task<Product> GetProductById(Guid productId);
    Task<Product> CreateProduct(CreateProduct createProduct, Guid LocationId);
    Task<IEnumerable<Product>> GetAllProducts(Guid LocationId, string? SearchByName = "", bool IncludeDeleted = false, CancellationToken cancellationToken = default);
    /// <summary>
    /// Take a product from inventory
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="Amount"></param>
    /// <returns></returns>
    Task<Product> TakeProduct(Guid productId, int Amount);

    /// <summary>
    /// Return a product to inventory
    /// </summary>
    /// <returns></returns>
    Task<Product> ReturnProduct(Guid productId, int Amount);

    /// <summary>
    /// Permanently delete a product from inventory
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    Task<Guid> DeleteProduct(Guid productId);
    /// <summary>
    /// Soft delete a product from inventory. Restorable.
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
     Task<Guid> SoftDeleteProduct(Guid productId);
    Task<Product> RestoreProduct(Guid productId);

}