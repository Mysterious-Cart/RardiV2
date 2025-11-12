using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Customer.Data;


[PrimaryKey("Id")]
public partial class Vehicle
{
    public Guid Id { get; set; }
    public string? VIN { get; set; }
    [Required]
    public string Make { get; set; } = null!;
    [Required]
    public string Model { get; set; } = null!;
    public int? Year { get; set; }
    public ICollection<CustomerProfile> Owners { get; set; } = [];

    public Vehicle()
    {
        Id = Guid.NewGuid();
    }
}