namespace Inventory.Api.Contracts;

public sealed record CreateItemRequest(
    string Code,
    string Name,
    string Unit,
    int MinStockLevel);

public sealed record ItemResponse(
    Guid Id,
    string Code,
    string Name,
    string Unit,
    int MinStockLevel,
    int CurrentStock,
    bool IsLowStock);
