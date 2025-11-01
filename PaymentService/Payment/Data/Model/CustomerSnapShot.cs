namespace Payment.Data;

public class CustomerSnapshot
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string PlateNumber { get; set; } = null!;
    // Reference to the original Customer entity at the time of transaction
    public Guid CustomerId { get; set; }
}