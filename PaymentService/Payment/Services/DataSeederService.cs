using Microsoft.EntityFrameworkCore;
using Payment.Data;

namespace Payment.Services;

public class DataSeederService
{
    private readonly PaymentContext _context;

    public DataSeederService(IDbContextFactory<PaymentContext> contextFactory)
    {
        _context = contextFactory.CreateDbContext();
    }

    public async Task SeedAsync()
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (!_context.PaymentTypes.Any())
            {
                var paymentTypes = new List<PaymentType>
                {
                    new () { Type = "Bank" },
                    new () { Type = "Riel" },
                    new () { Type = "Baht" },
                    new () { Type = "Dollar" }
                };

                _context.PaymentTypes.AddRange(paymentTypes);
                await _context.SaveChangesAsync();
            }
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
        
    }
}