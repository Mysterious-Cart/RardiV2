namespace Payment.Data;

public class Transaction
{
    public Guid Id { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Optional link to Customer entity if available, null if customer is a walk-in or no longer available
    public Guid? CustomerId { get; set; }
    public Guid CustomerSnapshotId { get; set; }
    public CustomerSnapshot? Customer { get; set; }
    public decimal Total { get; set; }
    // User who processed the transaction
    public Guid TransactBy { get; set; }
    public ICollection<PaymentInfo> PaymentInfos { get; set; } = [];
    public ICollection<TransactionItem> Items { get; set; } = [];
}