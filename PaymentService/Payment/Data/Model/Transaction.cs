namespace Payment.Data;

public class Transaction
{
    public Guid Id { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Optional link to Customer Cart entity if available, null if customer is a walk-in or no longer available
    /// </summary>
    public Guid? CartID { get; set; }
    public string PlateNumber { get; set; } = null!;
    public decimal Total { get; set; }
    /// <summary>
    /// User who processed the transaction
    /// </summary>
    public Guid TransactBy { get; set; }
    public ICollection<PaymentInfo> PaymentInfos { get; set; } = [];
    public ICollection<TransactionItem> Items { get; set; } = [];
}