using Payment.Data.DataLoader;
using Payment.Services;

namespace Payment.Graphql;

public class Query
{
    [UseFiltering]
    [UseSorting]
    [UsePaging]
    public async Task<IEnumerable<TransactionOverview>> GetAllTransactionsAsOverviewAsync(
        TransactionOverviewLoader transactionOverviewLoader,
        Guid Id,
        CancellationToken cancellationToken = default)
    {
        var transactions = await transactionOverviewLoader.LoadAsync(Id, cancellationToken);
        return transactions?.ToList() ?? [];
    }


}