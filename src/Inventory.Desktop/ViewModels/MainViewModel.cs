using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Inventory.Desktop.Models;
using Inventory.Desktop.Services;

namespace Inventory.Desktop.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly InventoryService _service = new();

    public ObservableCollection<Item> Items { get; }
    public ObservableCollection<Activity> Activities { get; }

    public MainViewModel()
    {
        Items = new ObservableCollection<Item>(_service.Items);
        Activities = new ObservableCollection<Activity>(_service.Activities);
        SelectedItem = Items.FirstOrDefault();
    }

    private Item? _selectedItem;
    public Item? SelectedItem
    {
        get => _selectedItem;
        set { _selectedItem = value; OnPropertyChanged(); }
    }

    public string NewCode { get; set; } = string.Empty;
    public string NewName { get; set; } = string.Empty;
    public string NewUnit { get; set; } = "عدد";
    public int NewMinStock { get; set; }

    public int DashboardItemsCount => Items.Count;
    public int DashboardTotalStock => Items.Sum(x => x.Stock);
    public int DashboardLowStockCount => Items.Count(x => x.Stock <= x.MinStock);

    public void CreateItem()
    {
        _service.AddItem(NewCode, NewName, NewUnit, NewMinStock);
        Reload();
    }

    public bool CreateIssue(int qty, string dep)
    {
        if (SelectedItem is null) return false;
        var ok = _service.AddIssue(SelectedItem.Id, qty, dep);
        Reload();
        return ok;
    }

    public void CreateReceipt(int qty, string supplier)
    {
        if (SelectedItem is null) return;
        _service.AddReceipt(SelectedItem.Id, qty, supplier);
        Reload();
    }

    private void Reload()
    {
        Items.Clear();
        foreach (var item in _service.Items.OrderBy(x => x.Name))
            Items.Add(item);

        Activities.Clear();
        foreach (var act in _service.Activities.Take(20))
            Activities.Add(act);

        OnPropertyChanged(nameof(DashboardItemsCount));
        OnPropertyChanged(nameof(DashboardTotalStock));
        OnPropertyChanged(nameof(DashboardLowStockCount));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? memberName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
}
