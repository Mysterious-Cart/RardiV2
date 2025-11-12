namespace Payment.Data.DataLoader;

using Mapster;
using Microsoft.EntityFrameworkCore;
using Payment.Data;
using Payment.Services;
public class TransactionOverviewLoader : BatchDataLoader<Guid, List<TransactionOverview>>
{
    private readonly PaymentContext _context;

    public TransactionOverviewLoader(
        IBatchScheduler batchScheduler,
        PaymentContext PaymentContext,
        DataLoaderOptions? options = null) : base(batchScheduler, options)
    {
        _context = PaymentContext;
    }

    protected override async Task<IReadOnlyDictionary<Guid, List<TransactionOverview>>> LoadBatchAsync(IReadOnlyList<Guid> keys, CancellationToken cancellationToken)
    {
        var transactions = _context.Transactions
            .Include(i => i.PaymentInfos)
            .ThenInclude(pi => pi.PaymentType)
            .ProjectToType<TransactionOverview>();

        return transactions.Where(t => keys.Contains(t.Id)).GroupBy(i => i.Id).ToDictionary(t => t.Key, t => t.ToList());
    }
}