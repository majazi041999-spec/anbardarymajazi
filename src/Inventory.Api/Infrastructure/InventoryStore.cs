using Inventory.Api.Domain;

namespace Inventory.Api.Infrastructure;

public sealed class InventoryStore
{
    private readonly List<Item> _items = [];
    private readonly List<StockReceipt> _receipts = [];
    private readonly object _lock = new();

    public IReadOnlyList<Item> GetItems()
    {
        lock (_lock)
        {
            return _items
                .OrderBy(x => x.Name)
                .ToList();
        }
    }

    public Item? GetItem(Guid id)
    {
        lock (_lock)
        {
            return _items.FirstOrDefault(x => x.Id == id);
        }
    }

    public Item? GetItemByCode(string code)
    {
        lock (_lock)
        {
            return _items.FirstOrDefault(x =>
                x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        }
    }

    public Item AddItem(Item item)
    {
        lock (_lock)
        {
            _items.Add(item);
            return item;
        }
    }

    public StockReceipt AddReceipt(StockReceipt receipt)
    {
        lock (_lock)
        {
            _receipts.Add(receipt);
            return receipt;
        }
    }

    public IReadOnlyList<StockReceipt> GetRecentReceipts(int take)
    {
        lock (_lock)
        {
            return _receipts
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(take)
                .ToList();
        }
    }
}
