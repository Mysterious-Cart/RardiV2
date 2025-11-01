using Customer.Data;

namespace Customer.Services;


public class CustomerService(
    CustomerContext context
)
{
    private readonly CustomerContext _context = context;

    public async Task AddCustomerAsync(CustomerProfile customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
    }

    public async Task<CustomerProfile?> GetCustomerByIdAsync(Guid id)
    {
        return await _context.Customers.FindAsync(id);
    }
}