using Microsoft.Win32;
using System.Windows;
using Inventory.Desktop.ViewModels;
using Forms = System.Windows.Forms;

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
            SetStatus("کد و نام کالا اجباری است.", true);
            return;
        }

        var msg = _vm.CreateItem();
        CodeBox.Clear();
        NameBox.Clear();
        SetStatus(msg, msg.Contains("تکراری"));
    }

    private void CreateReceipt_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(ReceiptQtyBox.Text, out var qty) || qty <= 0)
        {
            SetStatus("تعداد رسید معتبر نیست.", true);
            return;
        }

        var msg = _vm.CreateReceipt(qty, SupplierBox.Text.Trim());
        SetStatus(msg, msg.Contains("پیدا نشد") || msg.Contains("ابتدا") || msg.Contains("بیشتر"));
    }

    private void CreateIssue_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(IssueQtyBox.Text, out var qty) || qty <= 0)
        {
            SetStatus("تعداد خروج معتبر نیست.", true);
            return;
        }

        var msg = _vm.CreateIssue(qty, DepartmentBox.Text.Trim());
        SetStatus(msg, msg.Contains("نیست") || msg.Contains("ابتدا") || msg.Contains("پیدا نشد"));
    }

    private void ImportCsv_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "انتخاب فایل CSV (Tbl_Product یا خروجی ساده)",
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() != true)
            return;

        var msg = _vm.ImportCsv(dialog.FileName);
        SetStatus(msg, msg.Contains("خطا") || msg.Contains("پیدا نشد"));
    }

    private void ImportAccessFolder_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new Forms.FolderBrowserDialog
        {
            Description = "پوشه خروجی CSVهای Access را انتخاب کنید (Tbl_Product, Tbl_Trans_Detail, Tbl_Output_Detail)",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog() != Forms.DialogResult.OK)
            return;

        var msg = _vm.ImportAccessFolder(dialog.SelectedPath);
        SetStatus(msg, msg.Contains("خطا") || msg.Contains("پیدا نشد"));
    }

    private void SetStatus(string text, bool isError = false)
    {
        StatusText.Text = text;
        StatusText.Foreground = isError
            ? System.Windows.Media.Brushes.OrangeRed
            : System.Windows.Media.Brushes.LightGreen;
    }
}
