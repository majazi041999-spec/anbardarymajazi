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
    public ObservableCollection<Item> LowStockItems { get; }

    public MainViewModel()
    {
        Items = new ObservableCollection<Item>(_service.Items.OrderBy(x => x.Name));
        Activities = new ObservableCollection<Activity>(_service.Activities);
        LowStockItems = new ObservableCollection<Item>(_service.Items.Where(x => x.Stock <= x.MinStock));
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
    public int DashboardLowStockCount => LowStockItems.Count;

    public string CreateItem()
    {
        var result = _service.AddItem(NewCode, NewName, NewUnit, NewMinStock);
        Reload();
        return result.message;
    }

    public string CreateIssue(int qty, string dep)
    {
        if (SelectedItem is null) return "ابتدا یک کالا انتخاب کنید.";
        var result = _service.AddIssue(SelectedItem.Id, qty, dep);
        Reload();
        return result.message;
    }

    public string CreateReceipt(int qty, string supplier)
    {
        if (SelectedItem is null) return "ابتدا یک کالا انتخاب کنید.";
        var result = _service.AddReceipt(SelectedItem.Id, qty, supplier);
        Reload();
        return result.message;
    }

    public string ImportCsv(string path)
    {
        var result = _service.ImportFromCsv(path);
        Reload();

        var msg = $"ایمپورت انجام شد | جدید: {result.imported} | بروزرسانی: {result.updated} | رد شده: {result.skipped}";
        if (result.errors.Count > 0)
            msg += $" | خطا: {result.errors[0]}";

        return msg;
    }

    public string ImportAccessFolder(string folderPath)
    {
        var msg = _service.ImportAccessFolder(folderPath);
        Reload();
        return msg;
    }

    private void Reload()
    {
        var currentSelectedId = SelectedItem?.Id;

        Items.Clear();
        foreach (var item in _service.Items.OrderBy(x => x.Name))
            Items.Add(item);

        LowStockItems.Clear();
        foreach (var item in _service.Items.Where(x => x.Stock <= x.MinStock).OrderBy(x => x.Stock))
            LowStockItems.Add(item);

        Activities.Clear();
        foreach (var act in _service.Activities.Take(40))
            Activities.Add(act);

        SelectedItem = Items.FirstOrDefault(x => x.Id == currentSelectedId) ?? Items.FirstOrDefault();

        OnPropertyChanged(nameof(DashboardItemsCount));
        OnPropertyChanged(nameof(DashboardTotalStock));
        OnPropertyChanged(nameof(DashboardLowStockCount));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? memberName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
}
