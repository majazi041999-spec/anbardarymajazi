namespace Inventory.Api.Domain;

public sealed class NamedEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; init; } = string.Empty;
}
