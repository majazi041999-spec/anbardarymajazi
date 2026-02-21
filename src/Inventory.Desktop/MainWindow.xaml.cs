using System.Windows;
using Inventory.Desktop.ViewModels;

namespace Inventory.Desktop;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
    }

    private void CreateItem_Click(object sender, RoutedEventArgs e)
    {
        _vm.NewCode = CodeBox.Text.Trim();
        _vm.NewName = NameBox.Text.Trim();
        _vm.NewUnit = UnitBox.Text.Trim();
        _vm.NewMinStock = int.TryParse(MinStockBox.Text, out var min) ? min : 0;

        if (string.IsNullOrWhiteSpace(_vm.NewCode) || string.IsNullOrWhiteSpace(_vm.NewName))
        {
            MessageBox.Show("کد و نام کالا اجباری است.");
            return;
        }

        _vm.CreateItem();
        CodeBox.Clear();
        NameBox.Clear();
    }

    private void CreateReceipt_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(ReceiptQtyBox.Text, out var qty) || qty <= 0)
        {
            MessageBox.Show("تعداد رسید معتبر نیست.");
            return;
        }

        _vm.CreateReceipt(qty, SupplierBox.Text.Trim());
    }

    private void CreateIssue_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(IssueQtyBox.Text, out var qty) || qty <= 0)
        {
            MessageBox.Show("تعداد خروج معتبر نیست.");
            return;
        }

        var ok = _vm.CreateIssue(qty, DepartmentBox.Text.Trim());
        if (!ok)
        {
            MessageBox.Show("موجودی کافی نیست.");
        }
    }
}
