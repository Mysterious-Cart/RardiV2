namespace Customer.Graphql;

using Assets.Domain;
using Services;
public class Query
{
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public async Task<List<Customer>> GetCustomers([Service] CustomerService customerService)
    {
        return await customerService.GetAllCustomers();
    }
    
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public async Task<List<Vehicle>> GetVehicles([Service] CustomerService customerService)
    {
        return await customerService.GetAllVehicles();
    }

    public async Task<Vehicle?> GetVehicleById(Guid id, [Service] CustomerService customerService)
    {
        return await customerService.GetVehicleById(id);
    }
    
}