using Inventory.Api.Infrastructure;

namespace Inventory.Api.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard").WithTags("Dashboard");

        group.MapGet("/summary", (InventoryStore store) => Results.Ok(store.GetDashboardSummary()));

        group.MapGet("/trend", (InventoryStore store, int days = 14) =>
        {
            days = Math.Clamp(days, 1, 90);
            return Results.Ok(store.GetDailyTrend(days));
        });

        group.MapGet("/activity", (InventoryStore store, int take = 20) =>
        {
            take = Math.Clamp(take, 1, 100);
            return Results.Ok(store.GetRecentActivities(take));
        });

        return app;
    }
}
