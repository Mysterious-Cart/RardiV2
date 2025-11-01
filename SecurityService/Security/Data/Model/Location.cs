using HotChocolate.ApolloFederation.Types;
using Microsoft.AspNetCore.Authorization;
using Security.Data;

namespace Security.Asset;
public class Location
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<User> Users { get; set; } = [];
}