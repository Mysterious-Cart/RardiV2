namespace Inventory.Assets.Domain;

public record CreateProduct(
    string Name,
    string Description,
    int Stock
);

public record Product(
    Guid Id,
    string Name,
    string Description,
    int Stock
);

