namespace Customer.Graphql;

using Assets.Domain;
using Services;

public class Mutation
{
    public async Task<Customer?> AddCustomer(
        CreateCustomerRequest createCustomerRequest,
        [Service] CustomerService customerService)
    {
        return await customerService.CreateCustomer(createCustomerRequest);
    }

    public async Task AddVehicleToCustomer(
        AddVehicleToCustomerRequest request,
        [Service] CustomerService customerService)
    {
        await customerService.AddVehicleToCustomer(request);
    }

    public async Task<Cart?> CreateCart(
        Guid CustomerId,
        Guid VehicleId,
        [Service] CustomerService customerService)
    {
        return await customerService.CreateCart(CustomerId, VehicleId);
    }

    public async Task<CartItem?> AddItemToCart(
        Guid CartId,
        Guid ProductId,
        int Quantity,
        decimal Price,
        [Service] CustomerService customerService)
    {
        return await customerService.AddItemToCart(CartId, ProductId, Quantity, Price);
    }

    public async Task AddVehicle(
        CreateVehicleRequest createVehicleRequest,
        [Service] CustomerService customerService)
    {
        await customerService.AddVehicle(createVehicleRequest);
    }
}