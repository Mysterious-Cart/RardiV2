namespace Security.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Security.Assets;

public class SecurityContext : IdentityDbContext<User, Role, Guid>
{
    public SecurityContext(DbContextOptions<SecurityContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("Security");
        base.OnModelCreating(builder);
        // Additional model configuration can go here
    }

    public DbSet<Location> Locations { get; set; }
}