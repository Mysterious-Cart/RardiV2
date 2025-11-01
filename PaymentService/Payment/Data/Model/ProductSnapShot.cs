namespace Payment.Data;

public class ProductSnapShot
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;

    //Creation Date of the Product
    public DateTime CreatedAt { get; set; }
    public ICollection<TransactionItem> TransactionItems { get; set; } = [];


}