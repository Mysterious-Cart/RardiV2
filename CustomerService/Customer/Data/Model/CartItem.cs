namespace Customer.Data;

public class CartItem
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    public CartItem()
    {
        Id = Guid.NewGuid();
    }   
}