namespace Inventory.Api.Domain;

public sealed class StockIssue
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ItemId { get; init; }
    public int Quantity { get; init; }
    public string DepartmentName { get; init; } = string.Empty;
    public string ReferenceNo { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}
