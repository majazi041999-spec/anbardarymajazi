namespace Inventory.Api.Contracts;

public sealed record DashboardSummaryResponse(
    int ItemsCount,
    int TotalStock,
    int LowStockItemsCount,
    int RecentReceiptsCount,
    int RecentIssuesCount);
