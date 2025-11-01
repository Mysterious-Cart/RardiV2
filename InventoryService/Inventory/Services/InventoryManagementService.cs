using Inventory.Assets;
using Inventory.Data;
using DTO = Inventory.Assets.Domain;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using HotChocolate.Subscriptions;

namespace Inventory.Services;

public class InventoryManagementService(
    ITopicEventSender eventSender,
    InventoryContext context,
    ILogger<InventoryManagementService> logger
)
{
    private readonly ITopicEventSender _eventSender = eventSender;
    private readonly InventoryContext _context = context;
    private readonly ILogger<InventoryManagementService> _logger = logger;

    
    public async Task<DTO.Product> GetProductById(Guid productId)
    {
        return (await _context.Products.FindAsync(productId))?.Adapt<DTO.Product>() ?? throw new ArgumentException("Product not found");
    }

    public async Task<DTO.Product> CreateProduct(DTO.CreateProduct createProduct)
    {
        try
        {
            // Valid Input
            ArgumentException.ThrowIfNullOrEmpty(createProduct.Name, nameof(createProduct.Name));

            // Check for duplicate product name (case-insensitive, whitespace-insensitive, Khmer character normalization)
            // By first normalizing the product name (Khmer, English, Mixed)
            var normalizedname = KhmerTextNormalizer.Normalize(createProduct.Name);
            // Checking If Product with the same normalized name already exists
            if (await _context.Products.AnyAsync(p => p.NormalizedName == normalizedname))
            {
                throw new ArgumentException("Product with the same name already exists.", nameof(createProduct));
            }

            // If not exists, proceed to create the product
            var product = new ProductEntity
            {
                Name = createProduct.Name,
                NormalizedName = normalizedname,
                Description = createProduct.Description,
                TotalStock = createProduct.Stock,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            
            // Add the product
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Check if Creation was successful, and return the created product as DTO
            return (await _context.Products.FindAsync(product.Id))?.Adapt<DTO.Product>()
                ?? throw new InvalidOperationException("Failed Create Product");
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Error creating product");
            throw;
        }

    }
    
    public async Task<List<DTO.Product>> GetAllProducts(CancellationToken cancellationToken = default, string? SearchByName = "", bool IncludeDeleted = false)
    {

        //TODO: Normalize SearchByName input for better search results
        cancellationToken.ThrowIfCancellationRequested();
        if (!IncludeDeleted)
        {
            // Retrieve all products from the database and map to DTO, Deleted Product is not included by default
            return await _context.Products
                .Where(p => string.IsNullOrEmpty(SearchByName) || p.Name.Contains(SearchByName))
                .ProjectToType<DTO.Product>()
                .ToListAsync(cancellationToken);
        }

        // If IncludeDeleted is true, retrieve all products including deleted ones
        return await _context.Products
            .IgnoreQueryFilters()
            .Where(p => string.IsNullOrEmpty(SearchByName) || p.Name.Contains(SearchByName))
            .ProjectToType<DTO.Product>()
            .ToListAsync(cancellationToken);
    }

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

            await UpdateProductStock(order.ProductProfile.Product.Id, order.Quantity);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex) when (ex is not InvalidOperationException and not ArgumentNullException)
        {
            _logger.LogError(ex, "Error completing order");
            throw;
        }
    }

    private async Task<DTO.Product?> UpdateProductStock(Guid productPfpId, int AmountToAdd)
    {
        try
        {
            if(AmountToAdd < 0)
            {
                throw new ArgumentException("Changes cannot be negative", nameof(AmountToAdd));
            }else if(AmountToAdd == 0)
            {
                throw new ArgumentException("Changes cannot be zero", nameof(AmountToAdd));
            }
            var product = await _context.ProductProfiles.Include(i => i.Product).FirstAsync(i => i.Id == productPfpId);
            if (product.Product is null)
            {
                _logger.LogError("Product not found for ProductProfile with ID: {ProductProfileId}. Reason: Missing Reference.", productPfpId);
                throw new NullReferenceException("Product not found for the given ProductProfile. This is most likely result from data inconsistency, missing reference.");
            }

            product.Quantity += AmountToAdd;
            product.UpdatedAt = DateTime.UtcNow;
            product.Product.TotalStock += AmountToAdd;
            product.Product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return product.Adapt<DTO.Product>();

        }catch(Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Error updating product stock");
            throw;
        }
        
    }

    public async Task ImportProduct(Guid Product, int Quantity)
    {
        
    }

    public async Task<Guid> SoftDeleteProduct(Guid productId)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);

            ArgumentNullException.ThrowIfNull(product, nameof(productId));
            ArgumentException.ThrowIfNullOrEmpty(productId.ToString(), nameof(productId));


            product.isDeleted = true;
            product.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return productId;
        }catch(Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Error soft deleting product");
            throw;
        }
        
    }
    
    public async Task<Guid> RestoreProduct(Guid productId)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);
        
            ArgumentNullException.ThrowIfNull(product, nameof(productId));
            ArgumentException.ThrowIfNullOrEmpty(productId.ToString(), nameof(productId));

            if (product.isDeleted)
            {
                throw new InvalidOperationException("Cannot restore a product that is not deleted.");
            }
            
            product.isDeleted = false;
            product.DeletedAt = null;

            await _context.SaveChangesAsync();
            return productId;
        }catch(Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Error restoring product");
            throw;
        }
        
    }
    public async Task<bool> DeleteProduct(Guid productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
    

}