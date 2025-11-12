using Microsoft.EntityFrameworkCore;

namespace Customer.Data;

public class CustomerContext : DbContext
{
    public CustomerContext(DbContextOptions<CustomerContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("Customer");

        builder.Entity<CustomerProfile>()
                .HasMany(c => c.Vehicles)
                .WithMany(v => v.Owners)
                .UsingEntity<CustomerVehicleProfile>();
        base.OnModelCreating(builder);
        // Additional model configuration can go here
    }
    public DbSet<CustomerProfile> Customers { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<CustomerVehicleProfile> CustomerVehicleProfiles { get; set; }
}