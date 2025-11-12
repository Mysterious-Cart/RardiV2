namespace Inventory.Assets.Interface;

public interface IOrderManagementService
{
    Task<Assets.Domain.Order> CreateOrder(Guid ProductSupplierProfileId, int Quantity, decimal? ImportPrice);
    Task CompleteOrder(Guid orderId);
    Task CancelOrder(Guid orderId);
    
}