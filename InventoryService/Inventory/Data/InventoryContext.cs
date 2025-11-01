using Inventory.Data;
using Microsoft.EntityFrameworkCore;
namespace Inventory.Data;
public class InventoryContext : DbContext
{
    public InventoryContext(DbContextOptions<InventoryContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("Inventory");
        modelBuilder.Entity<ProductEntity>()
            .HasMany(p => p.Suppliers)
            .WithMany(s => s.Products)
            .UsingEntity<ProductSupplierProfiles>();

        modelBuilder.Entity<ProductEntity>()
            .HasQueryFilter(p => !p.isDeleted);

        modelBuilder.Entity<ProductProfile>()
            .HasQueryFilter(p => !p.Product!.isDeleted && !p.isDeleted);
        modelBuilder.Entity<ProductSupplierProfiles>()
            .HasQueryFilter(ps => !ps.Product!.isDeleted);
    }

    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<ProductProfile> ProductProfiles { get; set; }
    public DbSet<ProductSupplierProfiles> ProductSupplierProfiles { get; set; }
    public DbSet<Category> Categories { get; set; }
    
}