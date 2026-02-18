using Inventory.Api.Contracts;
using Inventory.Api.Infrastructure;

namespace Inventory.Api.Endpoints;

public static class IssueEndpoints
{
    public static IEndpointRouteBuilder MapIssueEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/issues").WithTags("Issues");

        group.MapGet("/recent", (InventoryStore store) =>
        {
            var response = store
                .GetRecentIssues(20)
                .Select(x =>
                {
                    var item = store.GetItem(x.ItemId);
                    return new IssueResponse(
                        x.Id,
                        x.ItemId,
                        item?.Name ?? "-",
                        x.Quantity,
                        x.DepartmentName,
                        x.ReferenceNo,
                        x.CreatedAtUtc);
                });

            return Results.Ok(response);
        });

        group.MapPost("/", (CreateIssueRequest request, InventoryStore store) =>
        {
            var errors = Validate(request);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            var result = store.AddIssue(
                request.ItemId,
                request.Quantity,
                request.DepartmentName.Trim(),
                request.ReferenceNo.Trim());

            if (result.IsNotFound)
            {
                return Results.NotFound(new { message = "کالای انتخاب‌شده پیدا نشد." });
            }

            if (result.IsInsufficientStock)
            {
                return Results.BadRequest(new { message = "موجودی کافی نیست." });
            }

            var issue = result.Issue!;
            var item = result.Item!;
            var response = new IssueResponse(
                issue.Id,
                issue.ItemId,
                item.Name,
                issue.Quantity,
                issue.DepartmentName,
                issue.ReferenceNo,
                issue.CreatedAtUtc);

            return Results.Created($"/api/issues/{issue.Id}", response);
        });

        return app;
    }

    private static Dictionary<string, string[]> Validate(CreateIssueRequest request)
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

        if (string.IsNullOrWhiteSpace(request.DepartmentName))
        {
            errors["departmentName"] = ["نام واحد دریافت‌کننده اجباری است."];
        }

        if (string.IsNullOrWhiteSpace(request.ReferenceNo))
        {
            errors["referenceNo"] = ["شماره سند اجباری است."];
        }

        return errors;
    }
}
