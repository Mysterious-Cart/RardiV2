namespace Inventory.Graphql.DataLoader;

using DTO = Inventory.Assets.Domain;
using Inventory.Services;
using Microsoft.EntityFrameworkCore;
using Inventory.Data;
using Mapster;

public class ProductLoader : BatchDataLoader<Guid, DTO.Product>
{
    public readonly InventoryContext _dbContext;
    public ProductLoader(
        IDbContextFactory<Inventory.Data.InventoryContext> dbContextFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null
    ) : base(batchScheduler, options)
    {
        _dbContext = dbContextFactory.CreateDbContext();
    }

    protected override async Task<IReadOnlyDictionary<Guid, DTO.Product>> LoadBatchAsync(IReadOnlyList<Guid> keys, CancellationToken cancellationToken)
    {
        var products = new Dictionary<Guid, DTO.Product>();
        var entities = await _dbContext.Products
            .Where(p => keys.Contains(p.Id))
            .ProjectToType<DTO.Product>()
            .ToListAsync(cancellationToken);
        return entities.ToDictionary(p => p.Id);
    }
}