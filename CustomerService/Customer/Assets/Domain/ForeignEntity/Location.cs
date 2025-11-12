using HotChocolate.ApolloFederation.Types;

namespace Customer.Assets;


[ExtendServiceType]
public class Location
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}