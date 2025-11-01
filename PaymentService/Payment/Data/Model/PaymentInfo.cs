namespace Payment.Data;

public class PaymentInfo
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public Guid PaymentTypeId { get; set; }
    public PaymentType PaymentType { get; set; } = null!;
}