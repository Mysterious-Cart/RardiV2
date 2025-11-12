namespace Inventory.Assets.Domain;

public record CreateProduct(
    string Name,
    string Description,
    int Stock,
    decimal Price,
    string? SKU = null
);

public record Product(
    Guid Id,
    string Name,
    decimal Price,
    string SKU,
    string Description,
    int Stock
);

