namespace Inventory.Api.Contracts;

public sealed record CreateNamedEntityRequest(string Name);

public sealed record NamedEntityResponse(Guid Id, string Name);
