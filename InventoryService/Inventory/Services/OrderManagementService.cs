namespace Inventory.Services;

using HotChocolate.Subscriptions;
using Inventory.Data;
using DTO = Assets.Domain;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Inventory.Assets;
using Inventory.Assets.Interface;
public class OrderManagementService(
    ITopicEventSender eventSender,
    IDbContextFactory<InventoryContext> context,
    IInventoryManagementService inventoryManagementService,
    ILogger<OrderManagementService> logger
) : IOrderManagementService
{
    private readonly ITopicEventSender _eventSender = eventSender;
    private readonly InventoryContext _context = context.CreateDbContext();
    private readonly IInventoryManagementService _inventoryManagementService = inventoryManagementService;
    private readonly ILogger<OrderManagementService> _logger = logger;

     public async Task OrderAdded(Order order)
    {
        await _eventSender.SendAsync(nameof(this.OrderAdded), order);
    }
    public async Task<DTO.Order> CreateOrder(Guid ProductSupplierProfileId, int Quantity, decimal? ImportPrice)
    {
        try
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                ProductSupplierProfileId = ProductSupplierProfileId,
                Quantity = Quantity,
                ImportPrice = ImportPrice,
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await OrderAdded(order);
            return (await _context.Orders.FindAsync(order.Id)).Adapt<DTO.Order>() ?? throw new Exception("Failed Create Order");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            throw;
        }

    }

    public async Task CompleteOrder(Guid orderId)
    {
        try
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            var order = await _context.Orders
            .Include(o => o.ProductProfile)
            .ThenInclude(pp => pp!.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

            ArgumentNullException.ThrowIfNull(order, nameof(orderId));
            if (order.Status != OperationStatus.Pending)
            {
                throw new InvalidOperationException("Only pending orders can be completed.");
            }
            ArgumentNullException.ThrowIfNull(order.ProductProfile, nameof(order.ProductProfile));
            ArgumentNullException.ThrowIfNull(order.ProductProfile!.Product, nameof(order.ProductProfile.Product));

            order.Status = OperationStatus.Completed;
            order.ImportPrice ??= order.ProductProfile.LatestImportPrice;
            order.ProductProfile.LatestImportPrice = order.ImportPrice.Value;

            await _inventoryManagementService.TakeProduct(order.ProductProfile.Product.Id, order.Quantity);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex) when (ex is not InvalidOperationException and not ArgumentNullException)
        {
            _logger.LogError(ex, "Error completing order");
            throw;
        }
    }

    public async Task CancelOrder(Guid orderId)
    {
        try
        {
            var order = await _context.Orders.FindAsync(orderId);

            ArgumentNullException.ThrowIfNull(order, nameof(orderId));
            if (order.Status != OperationStatus.Pending)
            {
                throw new InvalidOperationException("Only pending orders can be cancelled.");
            }

            order.Status = OperationStatus.Cancelled;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not InvalidOperationException and not ArgumentNullException)
        {
            _logger.LogError(ex, "Error cancelling order");
            throw;
        }
    }
}