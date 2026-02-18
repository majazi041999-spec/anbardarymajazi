using Inventory.Api.Infrastructure;

namespace Inventory.Api.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard").WithTags("Dashboard");

        group.MapGet("/summary", (InventoryStore store) =>
        {
            return Results.Ok(store.GetDashboardSummary());
        });

        return app;
    }
}
