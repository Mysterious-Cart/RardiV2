using Microsoft.EntityFrameworkCore;

namespace Payment.Data;

public class PaymentContext : DbContext
{
    public PaymentContext(DbContextOptions<PaymentContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("Payment");
    }

    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<TransactionItem> TransactionItems { get; set; } = null!;
    public DbSet<PaymentInfo> PaymentInfos { get; set; } = null!;
    public DbSet<PaymentType> PaymentTypes { get; set; } = null!;
}