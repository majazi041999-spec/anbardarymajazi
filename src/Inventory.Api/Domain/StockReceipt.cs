namespace Inventory.Api.Domain;

public sealed class StockReceipt
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ItemId { get; init; }
    public int Quantity { get; init; }
    public string SupplierName { get; init; } = string.Empty;
    public string ReferenceNo { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}
