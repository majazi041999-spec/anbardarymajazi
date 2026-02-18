namespace Inventory.Api.Domain;

public sealed class Item
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Unit { get; init; } = "عدد";
    public int MinStockLevel { get; init; }
    public int CurrentStock { get; private set; }

    public void IncreaseStock(int quantity) => CurrentStock += quantity;

    public bool TryDecreaseStock(int quantity)
    {
        if (quantity <= 0 || CurrentStock < quantity)
        {
            return false;
        }

        CurrentStock -= quantity;
        return true;
    }
}
