using Inventory.Api.Contracts;
using Inventory.Api.Infrastructure;

namespace Inventory.Api.Endpoints;

public static class MasterDataEndpoints
{
    public static IEndpointRouteBuilder MapMasterDataEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/masters").WithTags("Masters");

        group.MapGet("/suppliers", (InventoryStore store) =>
        {
            var response = store.GetSuppliers().Select(x => new NamedEntityResponse(x.Id, x.Name));
            return Results.Ok(response);
        });

        group.MapPost("/suppliers", (CreateNamedEntityRequest request, InventoryStore store) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["name"] = ["نام تامین‌کننده اجباری است."]
                });
            }

            var entity = store.AddSupplier(request.Name.Trim());
            return Results.Created($"/api/masters/suppliers/{entity.Id}", new NamedEntityResponse(entity.Id, entity.Name));
        });

        group.MapGet("/departments", (InventoryStore store) =>
        {
            var response = store.GetDepartments().Select(x => new NamedEntityResponse(x.Id, x.Name));
            return Results.Ok(response);
        });

        group.MapPost("/departments", (CreateNamedEntityRequest request, InventoryStore store) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["name"] = ["نام واحد اجباری است."]
                });
            }

            var entity = store.AddDepartment(request.Name.Trim());
            return Results.Created($"/api/masters/departments/{entity.Id}", new NamedEntityResponse(entity.Id, entity.Name));
        });

        return app;
    }
}
