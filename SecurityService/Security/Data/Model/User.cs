namespace Security.Data;

using Microsoft.AspNetCore.Identity;
using HotChocolate.ApolloFederation.Types;
using HotChocolate.ApolloFederation.Resolvers;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
/// Represents a user
/// </summary>
public partial class User : IdentityUser<Guid>, IEquatable<User>
{
    [ID]
    [Key]
    public override Guid Id { get => base.Id; set => base.Id = value; }

    [GraphQLIgnore]
    public override string? PasswordHash { get => base.PasswordHash; set => base.PasswordHash = value; }
    [GraphQLIgnore]
    public override int AccessFailedCount { get => base.AccessFailedCount; set => base.AccessFailedCount = value; }
    public override string? ConcurrencyStamp { get => base.ConcurrencyStamp; set => base.ConcurrencyStamp = value; }

    [GraphQLIgnore]
    public override string? NormalizedEmail { get => base.NormalizedEmail; set => base.NormalizedEmail = value; }
    public override string? Email { get => base.Email; set => base.Email = value; }

    [GraphQLIgnore]
    public override string? NormalizedUserName { get => base.NormalizedUserName; set => base.NormalizedUserName = value; }
    public override string? SecurityStamp { get => base.SecurityStamp; set => base.SecurityStamp = value; }
    public override DateTimeOffset? LockoutEnd { get => base.LockoutEnd; set => base.LockoutEnd = value; }

    public override bool PhoneNumberConfirmed { get => base.PhoneNumberConfirmed; set => base.PhoneNumberConfirmed = value; }
    public override bool TwoFactorEnabled { get => base.TwoFactorEnabled; set => base.TwoFactorEnabled = value; }
    public ICollection<Role> Roles { get; set; } = new List<Role>();
    [GraphQLIgnore]
    public Guid LocationId { get; set; }
    public Location? Location { get; set; }
    public bool Equals(User? other)
    {
        if (other is null) return false;
        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj is User user && Equals(user);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}