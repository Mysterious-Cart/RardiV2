using Customer.Data;
using Mapster;
using Microsoft.EntityFrameworkCore;
using DTO = Customer.Assets.Domain;
namespace Customer.Services;


public class CustomerService(
    CustomerContext context
)
{
    private readonly CustomerContext _context = context;

    public async Task<DTO.Customer?> CreateCustomer(DTO.CreateCustomerRequest createCustomerRequest)
    {
        var customerProfile = createCustomerRequest.Adapt<CustomerProfile>();
        _context.Customers.Add(customerProfile);
        await _context.SaveChangesAsync();
        return customerProfile.Adapt<DTO.Customer>();
    }
    public async Task<DTO.Customer?> AddVehicleToCustomer(DTO.AddVehicleToCustomerRequest request)
    {
        var customer = await _context.Customers
            .Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId);

        ArgumentException.ThrowIfNullOrEmpty(nameof(request), "Customer not found");

        var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
        ArgumentException.ThrowIfNullOrEmpty(nameof(request), "Vehicle not found");
        // already null checked above
        customer!.Vehicles.Add(vehicle!);
        await _context.SaveChangesAsync();
        return customer.Adapt<DTO.Customer>();
    }

    public async Task<List<DTO.Customer>> GetAllCustomers()
    {
        var customers = await _context.Customers.ToListAsync();
        return customers.Adapt<List<DTO.Customer>>();
    }

    public async Task<List<DTO.Vehicle>> GetAllVehicles()
    {
        var vehicles = await _context.Vehicles.ToListAsync();
        return vehicles.Adapt<List<DTO.Vehicle>>();
    }

    public async Task<DTO.Vehicle?> GetVehicleById(Guid id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        return vehicle?.Adapt<DTO.Vehicle>();
    }
    public async Task AddVehicle(DTO.CreateVehicleRequest createVehicleRequest)
    {
        // Check for existing vehicle with same VIN or same Make, Model, Year
        var SameVIN = await _context.Vehicles.AnyAsync(v => v.VIN == createVehicleRequest.VIN);
        if (SameVIN)
        {
            throw new ArgumentException("A vehicle with the same VIN already exists.");
        }
        var SameModelYear = await _context.Vehicles.AnyAsync(v => v.Make == createVehicleRequest.Make && v.Model == createVehicleRequest.Model && v.Year == createVehicleRequest.Year);
        if (SameModelYear)
        {
            throw new ArgumentException("A vehicle with the same Make, Model, and Year already exists.");
        }

        var vehicle = createVehicleRequest.Adapt<Vehicle>();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();
    }
    public async Task<DTO.Customer?> GetCustomerById(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        return customer?.Adapt<DTO.Customer>();
    }

    public async Task<DTO.Cart?> CreateCart(Guid CustomerId, Guid VehicleId)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == CustomerId);
        ArgumentException.ThrowIfNullOrEmpty(nameof(CustomerId), "Customer not found");

        var vehicle = await _context.Vehicles.FindAsync(VehicleId);
        ArgumentException.ThrowIfNullOrEmpty(nameof(VehicleId), "Vehicle not found");

        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            VehicleId = VehicleId,
            CreatedAt = DateTime.UtcNow
        };

        var customerVehicleProfile = await _context.CustomerVehicleProfiles
            .FirstOrDefaultAsync(cv => cv.CustomerId == CustomerId && cv.VehicleId == VehicleId);

        if (customerVehicleProfile == null)
        {
            await AddVehicleToCustomer(new DTO.AddVehicleToCustomerRequest
            (
                CustomerId,
                VehicleId
            ));

            customerVehicleProfile = await _context.CustomerVehicleProfiles
                .FirstAsync(cv => cv.CustomerId == CustomerId && cv.VehicleId == VehicleId);
        }

        customerVehicleProfile.Carts.Add(cart);
        await _context.SaveChangesAsync();
        return cart.Adapt<DTO.Cart>();
    }

    public async Task<DTO.CartItem?> AddItemToCart(Guid ProductId, Guid CartId, int Quantity, decimal Price)
    {
        // Check if cart exists
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == CartId);

        ArgumentException.ThrowIfNullOrEmpty(nameof(CartId), "Cart not found");

        //TODO: Check if product exists in product service

        // Create new cart item
        var cartItem = new CartItem
        {
            ProductId = ProductId,
            Quantity = Quantity,
            Price = Price
        };

        cart!.Items.Add(cartItem);
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return cartItem.Adapt<DTO.CartItem>();
        
    }
}