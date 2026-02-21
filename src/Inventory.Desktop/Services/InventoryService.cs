using Inventory.Desktop.Models;

namespace Inventory.Desktop.Services;

public sealed class InventoryService
{
    public List<Item> Items { get; } =
    [
        new Item { Code = "A-100", Name = "کاغذ A4", Unit = "بسته", MinStock = 5, Stock = 12 },
        new Item { Code = "P-210", Name = "پرینتر لیزری", Unit = "عدد", MinStock = 1, Stock = 2 }
    ];

    public List<Activity> Activities { get; } = [];

    public void AddItem(string code, string name, string unit, int minStock)
    {
        Items.Add(new Item
        {
            Code = code,
            Name = name,
            Unit = string.IsNullOrWhiteSpace(unit) ? "عدد" : unit,
            MinStock = minStock,
            Stock = 0
        });
    }

    public void AddReceipt(Guid itemId, int qty, string supplier)
    {
        var item = Items.First(x => x.Id == itemId);
        item.Stock += qty;
        Activities.Insert(0, new Activity { Type = "ورود", ItemName = item.Name, Quantity = qty, Party = supplier });
    }

    public bool AddIssue(Guid itemId, int qty, string department)
    {
        var item = Items.First(x => x.Id == itemId);
        if (item.Stock < qty) return false;
        item.Stock -= qty;
        Activities.Insert(0, new Activity { Type = "خروج", ItemName = item.Name, Quantity = qty, Party = department });
        return true;
    }
}
