using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Customer.Data;

[PrimaryKey("CustomerId", "VehicleId")]
public partial class CustomerVehicleProfile
{
    [ForeignKey("CustomerId")]
    public Guid CustomerId { get; set; }
    public CustomerProfile Customer { get; set; } = null!;
    
    [ForeignKey("VehicleId")]
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    public string PlateNumber { get; set; } = null!;
    public string Description { get; set; } = null!;

    public ICollection<Cart> Carts { get; set; } = [];

}