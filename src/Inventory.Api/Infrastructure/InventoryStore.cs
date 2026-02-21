using Inventory.Api.Contracts;
using Inventory.Api.Domain;

namespace Inventory.Api.Infrastructure;

public sealed class InventoryStore
{
    private readonly List<Item> _items = [];
    private readonly List<StockReceipt> _receipts = [];
    private readonly List<StockIssue> _issues = [];
    private readonly List<NamedEntity> _suppliers = [
        new NamedEntity { Name = "تامین‌کننده پیش‌فرض" }
    ];
    private readonly List<NamedEntity> _departments = [
        new NamedEntity { Name = "واحد اداری" }
    ];
    private readonly object _lock = new();

    public IReadOnlyList<Item> GetItems()
    {
        lock (_lock)
        {
            return _items.OrderBy(x => x.Name).ToList();
        }
    }

    public IReadOnlyList<LowStockItemResponse> GetLowStockItems()
    {
        lock (_lock)
        {
            return _items
                .Where(x => x.CurrentStock <= x.MinStockLevel)
                .OrderBy(x => x.CurrentStock)
                .Select(x => new LowStockItemResponse(
                    x.Id,
                    x.Code,
                    x.Name,
                    x.Unit,
                    x.CurrentStock,
                    x.MinStockLevel,
                    Math.Max(0, x.MinStockLevel - x.CurrentStock)))
                .ToList();
        }
    }


    public IReadOnlyList<NamedEntity> GetSuppliers()
    {
        lock (_lock)
        {
            return _suppliers.OrderBy(x => x.Name).ToList();
        }
    }

    public IReadOnlyList<NamedEntity> GetDepartments()
    {
        lock (_lock)
        {
            return _departments.OrderBy(x => x.Name).ToList();
        }
    }

    public NamedEntity AddSupplier(string name)
    {
        lock (_lock)
        {
            var existing = _suppliers.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                return existing;
            }

            var entity = new NamedEntity { Name = name };
            _suppliers.Add(entity);
            return entity;
        }
    }

    public NamedEntity AddDepartment(string name)
    {
        lock (_lock)
        {
            var existing = _departments.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                return existing;
            }

            var entity = new NamedEntity { Name = name };
            _departments.Add(entity);
            return entity;
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
            AddSupplier(supplierName);

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
                return AddIssueResult.InsufficientStock(item);
            }

            AddDepartment(departmentName);

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

    public IReadOnlyList<StockMovementResponse> GetItemMovements(Guid itemId, int take)
    {
        lock (_lock)
        {
            var item = _items.FirstOrDefault(x => x.Id == itemId);
            if (item is null)
            {
                return [];
            }

            var receiptMovements = _receipts
                .Where(x => x.ItemId == itemId)
                .Select(x => new StockMovementResponse(
                    itemId,
                    item.Name,
                    "receipt",
                    x.Quantity,
                    x.SupplierName,
                    x.ReferenceNo,
                    x.CreatedAtUtc));

            var issueMovements = _issues
                .Where(x => x.ItemId == itemId)
                .Select(x => new StockMovementResponse(
                    itemId,
                    item.Name,
                    "issue",
                    x.Quantity,
                    x.DepartmentName,
                    x.ReferenceNo,
                    x.CreatedAtUtc));

            return receiptMovements
                .Concat(issueMovements)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(take)
                .ToList();
        }
    }


    public IReadOnlyList<StockMovementResponse> GetRecentActivities(int take)
    {
        lock (_lock)
        {
            var itemNameMap = _items.ToDictionary(x => x.Id, x => x.Name);

            var receipts = _receipts.Select(x => new StockMovementResponse(
                x.ItemId,
                itemNameMap.GetValueOrDefault(x.ItemId, "-"),
                "receipt",
                x.Quantity,
                x.SupplierName,
                x.ReferenceNo,
                x.CreatedAtUtc));

            var issues = _issues.Select(x => new StockMovementResponse(
                x.ItemId,
                itemNameMap.GetValueOrDefault(x.ItemId, "-"),
                "issue",
                x.Quantity,
                x.DepartmentName,
                x.ReferenceNo,
                x.CreatedAtUtc));

            return receipts
                .Concat(issues)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(take)
                .ToList();
        }
    }

    public IReadOnlyList<DailyTrendPointResponse> GetDailyTrend(int days)
    {
        lock (_lock)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var from = today.AddDays(-days + 1);

            var receiptMap = _receipts
                .GroupBy(x => DateOnly.FromDateTime(x.CreatedAtUtc))
                .ToDictionary(x => x.Key, x => x.Sum(y => y.Quantity));

            var issueMap = _issues
                .GroupBy(x => DateOnly.FromDateTime(x.CreatedAtUtc))
                .ToDictionary(x => x.Key, x => x.Sum(y => y.Quantity));

            var result = new List<DailyTrendPointResponse>();
            for (var date = from; date <= today; date = date.AddDays(1))
            {
                receiptMap.TryGetValue(date, out var receiptQty);
                issueMap.TryGetValue(date, out var issueQty);
                result.Add(new DailyTrendPointResponse(date, receiptQty, issueQty));
            }

            return result;
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
    public static AddIssueResult InsufficientStock(Item item) => new(null, item, false, true);
    public static AddIssueResult Success(StockIssue issue, Item item) => new(issue, item, false, false);
}
