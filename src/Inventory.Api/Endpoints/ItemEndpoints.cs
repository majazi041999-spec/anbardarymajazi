using Inventory.Api.Contracts;
using Inventory.Api.Domain;
using Inventory.Api.Infrastructure;

namespace Inventory.Api.Endpoints;

public static class ItemEndpoints
{
    public static IEndpointRouteBuilder MapItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/items").WithTags("Items");

        group.MapGet("/", (InventoryStore store) =>
        {
            var response = store
                .GetItems()
                .Select(ToResponse);

            return Results.Ok(response);
        });

        group.MapPost("/", (CreateItemRequest request, InventoryStore store) =>
        {
            var errors = Validate(request);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            if (store.GetItemByCode(request.Code) is not null)
            {
                return Results.Conflict(new { message = "کد کالا تکراری است." });
            }

            var item = new Item
            {
                Code = request.Code.Trim(),
                Name = request.Name.Trim(),
                Unit = string.IsNullOrWhiteSpace(request.Unit) ? "عدد" : request.Unit.Trim(),
                MinStockLevel = request.MinStockLevel
            };

            store.AddItem(item);

            return Results.Created($"/api/items/{item.Id}", ToResponse(item));
        });

        return app;
    }

    private static Dictionary<string, string[]> Validate(CreateItemRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            errors["code"] = ["کد کالا اجباری است."];
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["name"] = ["نام کالا اجباری است."];
        }

        if (request.MinStockLevel < 0)
        {
            errors["minStockLevel"] = ["حداقل موجودی نمی‌تواند منفی باشد."];
        }

        return errors;
    }

    private static ItemResponse ToResponse(Item item)
        => new(
            item.Id,
            item.Code,
            item.Name,
            item.Unit,
            item.MinStockLevel,
            item.CurrentStock,
            item.CurrentStock <= item.MinStockLevel);
}
