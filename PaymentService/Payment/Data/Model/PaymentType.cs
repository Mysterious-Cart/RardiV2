namespace Payment.Data;

public class PaymentType
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = [];

}