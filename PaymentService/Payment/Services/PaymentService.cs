namespace Payment.Services;

using Mapster;
using Microsoft.EntityFrameworkCore;
using Payment.Asset.ForeignEntity;
using Payment.Data;

public class PaymentService
{
    private readonly ILogger<PaymentService> _logger;
    private readonly PaymentContext _context;

    public PaymentService(ILogger<PaymentService> logger, PaymentContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<Transaction?> GetTransactionByIdAsync(Guid id)
    {
        var transaction = await _context.Transactions
            .Include(i => i.PaymentInfos)
            .ThenInclude(pi => pi.PaymentType)
            .ProjectToType<Transaction>()
            .FirstOrDefaultAsync(t => t.Id == id) ?? throw new ArgumentException("Transaction not found");
        return transaction;
    }

    public async Task CreateTransaction(Cart customerCart, string Description)
    {

        var entity = new Data.Transaction
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Total = customerCart.Total,
            CartID = customerCart.Id,
            Description = Description,
            TransactBy = customerCart.CreatedBy
        };
    }
    
}

public record TransactionOverview(Guid Id, decimal Total, DateTime CreatedAt);
public record Transaction(Guid Id, decimal Total, DateTime CreatedAt, List<PaymentInfo> PaymentInfos, List<TransactionItem> Items);
