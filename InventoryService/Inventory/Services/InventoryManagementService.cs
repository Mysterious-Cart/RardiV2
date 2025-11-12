using Inventory.Assets;
using Inventory.Data;
using DTO = Inventory.Assets.Domain;
using Mapster;
using Microsoft.EntityFrameworkCore;
using HotChocolate.Subscriptions;
using Inventory.Assets.Interface;
namespace Inventory.Services;

public class InventoryManagementService(
    ITopicEventSender eventSender,
    IDbContextFactory<InventoryContext> context,
    ILogger<InventoryManagementService> logger
) : IInventoryManagementService
{
    private readonly ITopicEventSender _eventSender = eventSender;
    private readonly InventoryContext _context = context.CreateDbContext();
    private readonly ILogger<InventoryManagementService> _logger = logger;


    public async Task<DTO.Product> GetProductById(Guid productId)
    {
        return (await _context.Products.FindAsync(productId))?.Adapt<DTO.Product>() ?? throw new ArgumentException("Product not found");
    }

    /// <summary>
    /// Creates a new product in the inventory system of a specific location.
    /// </summary>
    /// <param name="createProduct"></param>
    /// <param name="LocationId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<DTO.Product> CreateProduct(DTO.CreateProduct createProduct, Guid LocationId)
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
                EnglishNormalizedName = createProduct.Name.ToUpperInvariant(),
                Description = createProduct.Description,
                TotalStock = createProduct.Stock,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            var productpfp = new ProductProfile
            {
                ProductId = product.Id,
                Quantity = createProduct.Stock,
                Price = createProduct.Price,
                SKU = createProduct.SKU ?? string.Empty, // generate SKU if not provided
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LocationId = LocationId
            };

            var transaction = await _context.Database.BeginTransactionAsync();
            // Add the product
            _context.Products.Add(product);
            _context.ProductProfiles.Add(productpfp);
            await _context.SaveChangesAsync();
            
            await transaction.CommitAsync();
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
    /// <summary>
    /// Retrieves all products in a specific inventory, with optional filtering by name and inclusion of deleted products.
    /// </summary>
    /// <param name="LocationId"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="SearchByName"></param>
    /// <param name="IncludeDeleted"></param>
    /// <returns></returns>
    public async Task<IEnumerable<DTO.Product>> GetAllProducts(
        Guid LocationId,
        string? SearchByName = "",
        bool IncludeDeleted = false,
        CancellationToken cancellationToken = default
    )
    {

        //TODO: Normalize SearchByName input for better search results
        cancellationToken.ThrowIfCancellationRequested();
        if (!IncludeDeleted)
        {
            // If IncludeDeleted is true, retrieve all products including deleted ones
            return await _context.Products
                .IgnoreQueryFilters()
                .Where(p => string.IsNullOrEmpty(SearchByName) || p.Name.Contains(SearchByName))
                .ProjectToType<DTO.Product>()
                .ToListAsync(cancellationToken);
        }

        var product = await _context.ProductProfiles
            .Where(p => string.IsNullOrEmpty(SearchByName) || p.Product!.Name.Contains(SearchByName))
            .Where(p => p.LocationId == LocationId)
            .AsNoTracking()
            .Select(i => new DTO.Product(
                i.Product!.Id,
                i.Product.Name,
                i.Price,
                i.SKU,
                i.Product.Description ?? "",
                i.Quantity
            )).ToListAsync(cancellationToken);

        return product;

        
    }

    public async Task<DTO.Product> UpdateProductStock(Guid productPfpId, int QuantityChange)
    {
        try
        {
            if(QuantityChange < 0)
            {
                throw new ArgumentException("Changes cannot be negative", nameof(QuantityChange));
            }else if(QuantityChange == 0)
            {
                throw new ArgumentException("Changes cannot be zero", nameof(QuantityChange));
            }
            var product = await _context.ProductProfiles.Include(i => i.Product).FirstAsync(i => i.Id == productPfpId);
            if (product.Product is null)
            {
                _logger.LogError("Product not found for ProductProfile with ID: {ProductProfileId}. Reason: Missing Reference.", productPfpId);
                throw new NullReferenceException("Product not found for the given ProductProfile. This is most likely result from data inconsistency, missing reference.");
            }

            product.Quantity += QuantityChange;
            product.UpdatedAt = DateTime.UtcNow;
            product.Product.TotalStock += QuantityChange;
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

    public async Task<DTO.Product> RestoreProduct(Guid productId)
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
            return product.Adapt<DTO.Product>();
        }catch(Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Error restoring product");
            throw;
        }
        
    }
    public async Task<Guid> DeleteProduct(Guid productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            return Guid.Empty;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return productId;
    }
    

}