namespace Customer.Assets.Domain;

public record Customer(Guid Id, string Name, string Email, string PhoneNumber, List<Vehicle> Vehicles)
{
    public override string ToString() => $"{Name} ({Email}, {PhoneNumber}) - Vehicles: {Vehicles.Count}";
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, Email, PhoneNumber, Vehicles);
    }
};
public record Vehicle(Guid Id, string? VIN, string Make, string Model, int? Year){
    public override string ToString() => $"{Year} {Make} {Model} (VIN: {VIN ?? "N/A"})";
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, VIN, Make, Model, Year);
    }
};
public record CreateCustomerRequest(string Name, string Email, string PhoneNumber);
public static class CreateCustomerRequestExtensions
{
    public static CreateCustomerRequest Clean(this CreateCustomerRequest request)
    {
        return new CreateCustomerRequest(
            Name: request.Name.Trim(),
            Email: request.Email.Trim(),
            PhoneNumber: request.PhoneNumber.Trim()
        );
    }
}

public record CreateVehicleRequest(string? VIN, string Make, string Model, int? Year);
public record AddVehicleToCustomerRequest(Guid CustomerId, Guid VehicleId);
