namespace Inventory.Desktop.Models;

public sealed class Item
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = "عدد";
    public int MinStock { get; set; }
    public int Stock { get; set; }
}

public sealed class Activity
{
    public DateTime Time { get; set; } = DateTime.Now;
    public string Type { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Party { get; set; } = string.Empty;
}
