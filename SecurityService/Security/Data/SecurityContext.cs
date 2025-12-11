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
        builder.Entity<User>().HasMany(i => i.Roles).WithMany(i => i.Users).UsingEntity<IdentityUserRole<Guid>>();
        base.OnModelCreating(builder);
        // Additional model configuration can go here
    }

    public DbSet<Location> Locations { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}