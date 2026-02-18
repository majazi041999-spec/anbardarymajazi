namespace Inventory.Api.Contracts;

public sealed record LowStockItemResponse(
    Guid ItemId,
    string Code,
    string Name,
    string Unit,
    int CurrentStock,
    int MinStockLevel,
    int Shortage);

public sealed record StockMovementResponse(
    Guid ItemId,
    string ItemName,
    string MovementType,
    int Quantity,
    string PartyName,
    string ReferenceNo,
    DateTime CreatedAtUtc);

public sealed record DailyTrendPointResponse(
    DateOnly Date,
    int ReceiptQuantity,
    int IssueQuantity);
