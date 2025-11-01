using Microsoft.EntityFrameworkCore;

namespace Customer.Data;


[PrimaryKey("Id")]
public partial class Vehicle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Make { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public ICollection<CustomerProfile> Owners { get; set; } = [];
    public ICollection<Category> Categories { get; set; } = [];
}