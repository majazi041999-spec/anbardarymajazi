using Inventory.Api.Contracts;
using Inventory.Api.Domain;

namespace Inventory.Api.Infrastructure;

public sealed class InventoryStore
{
    private readonly List<Item> _items = [];
    private readonly List<StockReceipt> _receipts = [];
    private readonly List<StockIssue> _issues = [];
    private readonly object _lock = new();

    public IReadOnlyList<Item> GetItems()
    {
        lock (_lock)
        {
            return _items.OrderBy(x => x.Name).ToList();
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
            return _items.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
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

    public (StockReceipt receipt, Item item)? AddReceipt(Guid itemId, int quantity, string supplierName, string referenceNo)
    {
        lock (_lock)
        {
            var item = _items.FirstOrDefault(x => x.Id == itemId);
            if (item is null)
            {
                return null;
            }

            item.IncreaseStock(quantity);

            var receipt = new StockReceipt
            {
                ItemId = itemId,
                Quantity = quantity,
                SupplierName = supplierName,
                ReferenceNo = referenceNo
            };

            _receipts.Add(receipt);
            return (receipt, item);
        }
    }

    public AddIssueResult AddIssue(Guid itemId, int quantity, string departmentName, string referenceNo)
    {
        lock (_lock)
        {
            var item = _items.FirstOrDefault(x => x.Id == itemId);
            if (item is null)
            {
                return AddIssueResult.NotFound;
            }

            if (!item.TryDecreaseStock(quantity))
            {
                return AddIssueResult.InsufficientStock;
            }

            var issue = new StockIssue
            {
                ItemId = itemId,
                Quantity = quantity,
                DepartmentName = departmentName,
                ReferenceNo = referenceNo
            };

            _issues.Add(issue);
            return AddIssueResult.Success(issue, item);
        }
    }

    public IReadOnlyList<StockReceipt> GetRecentReceipts(int take)
    {
        lock (_lock)
        {
            return _receipts.OrderByDescending(x => x.CreatedAtUtc).Take(take).ToList();
        }
    }

    public IReadOnlyList<StockIssue> GetRecentIssues(int take)
    {
        lock (_lock)
        {
            return _issues.OrderByDescending(x => x.CreatedAtUtc).Take(take).ToList();
        }
    }

    public DashboardSummaryResponse GetDashboardSummary()
    {
        lock (_lock)
        {
            return new DashboardSummaryResponse(
                ItemsCount: _items.Count,
                TotalStock: _items.Sum(x => x.CurrentStock),
                LowStockItemsCount: _items.Count(x => x.CurrentStock <= x.MinStockLevel),
                RecentReceiptsCount: _receipts.Count(x => x.CreatedAtUtc >= DateTime.UtcNow.AddDays(-7)),
                RecentIssuesCount: _issues.Count(x => x.CreatedAtUtc >= DateTime.UtcNow.AddDays(-7)));
        }
    }
}


public sealed record AddIssueResult(StockIssue? Issue, Item? Item, bool IsNotFound, bool IsInsufficientStock)
{
    public static AddIssueResult NotFound => new(null, null, true, false);
    public static AddIssueResult InsufficientStock => new(null, null, false, true);
    public static AddIssueResult Success(StockIssue issue, Item item) => new(issue, item, false, false);
}
