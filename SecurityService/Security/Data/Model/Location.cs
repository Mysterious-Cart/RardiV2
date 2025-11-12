using HotChocolate.ApolloFederation.Resolvers;
using HotChocolate.ApolloFederation.Types;
using Microsoft.AspNetCore.Authorization;
using Security.Data;

namespace Security.Data;
public class Location
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    [GraphQLIgnore]
    public bool isDeleted { get; set; } = false;

    public ICollection<User> Users { get; set; } = [];
    
    [ReferenceResolver]
    public async static Task<Location?> ResolveLocation(
        Guid locationId,
        [Service] SecurityContext context)
    {
        return await context.Locations.FindAsync(locationId);
    }
}