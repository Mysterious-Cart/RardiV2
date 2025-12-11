namespace Inventory.Assets.Interface;

public interface IOrderManagementService
{
    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="ProductSupplierProfileId"></param>
    /// <param name="Quantity"></param>
    /// <param name="ImportPrice"></param>
    /// <returns></returns>
    Task<Domain.Order> CreateOrder(Guid ProductSupplierProfileId, int Quantity, decimal? ImportPrice);
    Task CompleteOrder(Guid orderId);
    Task CancelOrder(Guid orderId);
    
}