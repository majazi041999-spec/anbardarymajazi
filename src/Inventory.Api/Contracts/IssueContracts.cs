namespace Inventory.Api.Contracts;

public sealed record CreateIssueRequest(
    Guid ItemId,
    int Quantity,
    string DepartmentName,
    string ReferenceNo);

public sealed record IssueResponse(
    Guid Id,
    Guid ItemId,
    string ItemName,
    int Quantity,
    string DepartmentName,
    string ReferenceNo,
    DateTime CreatedAtUtc);
