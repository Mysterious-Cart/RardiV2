namespace Inventory.Assets.Domain;

public record Order
(
    Guid Id,
    Guid ProductSupplierProfileId,
    OperationStatus Status,
    int Quantity,
    decimal? ImportPrice,
    DateTime CreatedAt
);