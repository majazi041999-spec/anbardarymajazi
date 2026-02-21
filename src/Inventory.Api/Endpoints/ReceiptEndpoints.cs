using Inventory.Api.Contracts;
using Inventory.Api.Infrastructure;

namespace Inventory.Api.Endpoints;

public static class ReceiptEndpoints
{
    public static IEndpointRouteBuilder MapReceiptEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/receipts").WithTags("Receipts");

        group.MapGet("/recent", (InventoryStore store) =>
        {
            var response = store
                .GetRecentReceipts(20)
                .Select(x =>
                {
                    var item = store.GetItem(x.ItemId);
                    return new ReceiptResponse(
                        x.Id,
                        x.ItemId,
                        item?.Name ?? "-",
                        x.Quantity,
                        x.SupplierName,
                        x.ReferenceNo,
                        x.CreatedAtUtc);
                });

            return Results.Ok(response);
        });

        group.MapPost("/", (CreateReceiptRequest request, InventoryStore store) =>
        {
            var errors = Validate(request);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            var result = store.AddReceipt(
                request.ItemId,
                request.Quantity,
                request.SupplierName.Trim(),
                request.ReferenceNo.Trim());

            if (result is null)
            {
                return Results.NotFound(new { message = "کالای انتخاب‌شده پیدا نشد." });
            }

            var (receipt, item) = result.Value;
            var response = new ReceiptResponse(
                receipt.Id,
                receipt.ItemId,
                item.Name,
                receipt.Quantity,
                receipt.SupplierName,
                receipt.ReferenceNo,
                receipt.CreatedAtUtc);

            return Results.Created($"/api/receipts/{receipt.Id}", response);
        });

        return app;
    }

    private static Dictionary<string, string[]> Validate(CreateReceiptRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.ItemId == Guid.Empty)
        {
            errors["itemId"] = ["انتخاب کالا اجباری است."];
        }

        if (request.Quantity <= 0)
        {
            errors["quantity"] = ["تعداد باید بزرگتر از صفر باشد."];
        }

        if (string.IsNullOrWhiteSpace(request.SupplierName))
        {
            errors["supplierName"] = ["نام تامین‌کننده اجباری است."];
        }

        if (string.IsNullOrWhiteSpace(request.ReferenceNo))
        {
            errors["referenceNo"] = ["شماره سند اجباری است."];
        }

        return errors;
    }
}
