using Microsoft.EntityFrameworkCore;
using Payment.Data;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using CsvHelper.Configuration;
using System.Threading.Tasks;
using System.Globalization;
using Model = Payment.Data;
namespace Payment.Services;

public class DataManagementService(IDbContextFactory<PaymentContext> contextFactory, ILogger<DataManagementService> logger)
{
    private readonly ILogger<DataManagementService> _logger = logger;
    private readonly PaymentContext paymentContext = contextFactory.CreateDbContext();


    public async Task ImportTransactionFromCsv(string filepath)
    {
        using var transaction = await paymentContext.Database.BeginTransactionAsync();
        try
        {
            using var reader = new StreamReader(filepath);
            using var csv = new CsvReader(reader, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ";"
            });

            // Import CSV Records to C# Object (easier to manipulate)
            var records = csv.GetRecords<CsvObject>().ToList();
            var paymentTypes = await paymentContext.PaymentTypes
                .ToDictionaryAsync(pt => pt.Type, pt => pt.Id);
            // Helper function to add payment info
            async Task<List<PaymentInfo>> AddPaymentInfos(CsvObject record)
            {
                var paymentInfos = new List<PaymentInfo>();
                if (record.Dollar > 0)
                {
                    paymentInfos.Add(new PaymentInfo
                    {
                        Id = Guid.NewGuid(),
                        Amount = record.Dollar,
                        PaymentTypeId = paymentTypes.Where(pt => pt.Key == "Dollar").Select(pt => pt.Value).First()
                    });

                }
                if (record.Baht > 0)
                {
                    paymentInfos.Add(new PaymentInfo
                    {
                        Id = Guid.NewGuid(),
                        Amount = record.Baht,
                        PaymentTypeId = paymentTypes
                            .Where(pt => pt.Key == "Baht")
                            .Select(pt => pt.Value).First()
                    });
                }
                if (record.Bank > 0)
                {
                    paymentInfos.Add(new PaymentInfo
                    {
                        Id = Guid.NewGuid(),
                        Amount = record.Bank,
                        PaymentTypeId = paymentTypes
                            .Where(pt => pt.Key == "Bank")
                            .Select(pt => pt.Value)
                            .First()
                    });
                }
                if (record.Riel > 0)
                {
                    paymentInfos.Add(new PaymentInfo
                    {
                        Id = Guid.NewGuid(),
                        Amount = record.Riel,
                        PaymentTypeId = paymentTypes.Where(pt => pt.Key == "Riel").Select(pt => pt.Value).First()
                    });
                }

                await paymentContext.PaymentInfos.AddRangeAsync(paymentInfos);
                await paymentContext.SaveChangesAsync();
                return paymentInfos;
            }

            // Import Transactions
            var transactions = new List<Model.Transaction>();
            foreach (var record in records)
            {
                var customersnapshotId = Guid.NewGuid();
                var transac = new Model.Transaction
                {
                    Id = Guid.NewGuid(),
                    CartID = Guid.Empty,
                    PaymentInfos = await AddPaymentInfos(record),
                    CreatedAt = ParseCashoutDate(record.CashoutDate),
                    TransactBy = Guid.Empty,
                    Total = record.Total,
                    IsDeleted = record.IsDeleted == 1,
                    DeletedAt = record.IsDeleted == 1 ? DateTime.UtcNow : null,
                };
                transactions.Add(transac);
            }

            await paymentContext.Transactions.AddRangeAsync(transactions);
            await paymentContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "An error occurred while importing transactions.");
            throw;
        }
    }

    private DateTime ParseCashoutDate(string cashoutDate)
    {
        if (string.IsNullOrEmpty(cashoutDate))
            return DateTime.UtcNow;

        // Handle different date formats from your CSV
        var dateFormats = new[]
        {
            "dd/MM/yyyy(HH:mm)",    // 01/02/2025(16:52)
            "dd/MM/yyyy(H:mm)",     // 01/02/2025(8:42)
            "dd/MM/yyyy(HH:m)",     // 01/02/2025(16:5)
            "dd/MM/yyyy(H:m)",      // 01/02/2025(8:5)
        };

        foreach (var format in dateFormats)
        {
            if (DateTime.TryParseExact(cashoutDate, format, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var result))
            {
                return result.ToUniversalTime();
            }
        }

        _logger.LogWarning("Could not parse date '{Date}', using current time", cashoutDate);
        return DateTime.UtcNow;
    }
}

public class CsvObject {
    public string? Plate { get; set; }
    public string? CashoutDate { get; set; }
    public decimal Total { get; set; }
    public decimal Bank { get; set; }
    public decimal Dollar { get; set; }
    public decimal Riel { get; set; }
    public decimal Baht { get; set; }
    public sbyte Company { get; set; }
    public string? User { get; set; }
    public sbyte IsDeleted { get; set; }
}