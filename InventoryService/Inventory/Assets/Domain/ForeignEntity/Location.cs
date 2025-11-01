using HotChocolate.ApolloFederation.Types;

namespace  Inventory.Assets;


[ExtendServiceType]
public class Location
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}