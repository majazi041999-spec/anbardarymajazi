namespace Inventory.Api.Contracts;

public sealed record CreateReceiptRequest(
    Guid ItemId,
    int Quantity,
    string SupplierName,
    string ReferenceNo);

public sealed record ReceiptResponse(
    Guid Id,
    Guid ItemId,
    string ItemName,
    int Quantity,
    string SupplierName,
    string ReferenceNo,
    DateTime CreatedAtUtc);
