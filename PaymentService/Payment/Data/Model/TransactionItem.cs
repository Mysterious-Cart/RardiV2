namespace Payment.Data;

public class TransactionItem
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public Transaction Transaction { get; set; } = null!;
    public Guid ProductId { get; set; }
    public ProductSnapShot? Product { get; set; }
    public string Description { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal Price { get; set; }

}