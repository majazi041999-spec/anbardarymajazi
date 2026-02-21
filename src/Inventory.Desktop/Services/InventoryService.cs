using System.Globalization;
using Inventory.Desktop.Models;

namespace Inventory.Desktop.Services;

public sealed class InventoryService
{
    public List<Item> Items { get; } =
    [
        new Item { Code = "A-100", Name = "کاغذ A4", Unit = "بسته", MinStock = 5, Stock = 12, GroupName = "ملزومات", LocationName = "قفسه 1" },
        new Item { Code = "P-210", Name = "پرینتر لیزری", Unit = "عدد", MinStock = 1, Stock = 2, GroupName = "تجهیزات", LocationName = "اتاق IT" }
    ];

    public List<Activity> Activities { get; } = [];

    public (bool ok, string message) AddItem(string code, string name, string unit, int minStock)
    {
        if (Items.Any(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase)))
            return (false, "کد کالا تکراری است.");

        Items.Add(new Item
        {
            Code = code,
            Name = name,
            Unit = string.IsNullOrWhiteSpace(unit) ? "عدد" : unit,
            MinStock = Math.Max(0, minStock),
            Stock = 0
        });

        Activities.Insert(0, new Activity { Type = "سیستم", ItemName = name, Quantity = 0, Party = "ثبت کالای جدید" });
        return (true, "کالا ثبت شد.");
    }

    public (bool ok, string message) AddReceipt(Guid itemId, int qty, string supplier)
    {
        var item = Items.FirstOrDefault(x => x.Id == itemId);
        if (item is null) return (false, "کالا پیدا نشد.");
        if (qty <= 0) return (false, "تعداد ورود باید بیشتر از صفر باشد.");

        item.Stock += qty;
        Activities.Insert(0, new Activity
        {
            Type = "ورود",
            ItemName = item.Name,
            Quantity = qty,
            Party = string.IsNullOrWhiteSpace(supplier) ? "تامین‌کننده نامشخص" : supplier
        });

        return (true, "رسید ثبت شد.");
    }

    public (bool ok, string message) AddIssue(Guid itemId, int qty, string department)
    {
        var item = Items.FirstOrDefault(x => x.Id == itemId);
        if (item is null) return (false, "کالا پیدا نشد.");
        if (qty <= 0) return (false, "تعداد خروج باید بیشتر از صفر باشد.");
        if (item.Stock < qty) return (false, "موجودی کافی نیست.");

        item.Stock -= qty;
        Activities.Insert(0, new Activity
        {
            Type = "خروج",
            ItemName = item.Name,
            Quantity = qty,
            Party = string.IsNullOrWhiteSpace(department) ? "واحد نامشخص" : department
        });

        return (true, "حواله خروج ثبت شد.");
    }

    public (int imported, int updated, int skipped, List<string> errors) ImportFromCsv(string path)
    {
        if (!File.Exists(path))
            return (0, 0, 0, ["فایل انتخاب‌شده پیدا نشد."]);

        return ImportProductTable(path);
    }

    public string ImportAccessFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            return "پوشه انتخاب‌شده وجود ندارد.";

        var csvFiles = Directory.GetFiles(folderPath, "*.csv", SearchOption.TopDirectoryOnly);
        if (csvFiles.Length == 0)
            return "فایل CSV در پوشه پیدا نشد.";

        var productFile = csvFiles.FirstOrDefault(f => Path.GetFileName(f).Contains("Tbl_Product", StringComparison.OrdinalIgnoreCase));
        var transDetailFile = csvFiles.FirstOrDefault(f => Path.GetFileName(f).Contains("Tbl_Trans_Detail", StringComparison.OrdinalIgnoreCase));
        var outputDetailFile = csvFiles.FirstOrDefault(f => Path.GetFileName(f).Contains("Tbl_Output_Detail", StringComparison.OrdinalIgnoreCase));

        var imported = 0;
        var updated = 0;
        var skipped = 0;
        var errors = new List<string>();

        if (productFile is not null)
        {
            var res = ImportProductTable(productFile);
            imported += res.imported;
            updated += res.updated;
            skipped += res.skipped;
            errors.AddRange(res.errors);
        }
        else
        {
            errors.Add("فایل Tbl_Product*.csv پیدا نشد.");
        }

        var inMoves = ApplyMovementTable(transDetailFile, +1, "ورود از تراکنش");
        var outMoves = ApplyMovementTable(outputDetailFile, -1, "خروج از خروجی");

        return $"ایمپورت Access کامل شد | محصول جدید: {imported} | بروزرسانی: {updated} | رد شده: {skipped} | ورود: {inMoves} | خروج: {outMoves}" +
               (errors.Count > 0 ? $" | هشدار: {errors[0]}" : string.Empty);
    }

    private (int imported, int updated, int skipped, List<string> errors) ImportProductTable(string path)
    {
        var rows = ReadCsv(path, out var headers);

        int idxCode = FindHeader(headers, "code", "کد", "itemcode", "product_code");
        int idxName = FindHeader(headers, "name", "نام", "itemname", "product_description");
        int idxUnit = FindHeader(headers, "unit", "واحد", "product_unit");
        int idxMin = FindHeader(headers, "minstock", "min_stock", "حداقل");
        int idxStock = FindHeader(headers, "stock", "quantity", "موجودی", "product_qty", "product_first_qty");
        int idxPrice = FindHeader(headers, "price", "product_price");
        int idxGroup = FindHeader(headers, "group_name", "group", "product_group");
        int idxLocation = FindHeader(headers, "location", "product_location", "location_name");

        if (idxCode < 0 || idxName < 0)
            return (0, 0, rows.Count, ["ستون‌های اجباری Code و Name پیدا نشدند."]);

        var imported = 0;
        var updated = 0;
        var skipped = 0;
        var errors = new List<string>();

        for (var i = 0; i < rows.Count; i++)
        {
            var cols = rows[i];
            var code = GetCol(cols, idxCode);
            var name = GetCol(cols, idxName);
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            {
                skipped++;
                continue;
            }

            try
            {
                var unit = idxUnit >= 0 ? GetCol(cols, idxUnit) : "عدد";
                var minStock = ParseInt(idxMin >= 0 ? GetCol(cols, idxMin) : "0");
                var stock = ParseInt(idxStock >= 0 ? GetCol(cols, idxStock) : "0");
                var price = ParseDecimal(idxPrice >= 0 ? GetCol(cols, idxPrice) : "0");
                var group = idxGroup >= 0 ? GetCol(cols, idxGroup) : string.Empty;
                var location = idxLocation >= 0 ? GetCol(cols, idxLocation) : string.Empty;

                var existing = Items.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (existing is null)
                {
                    Items.Add(new Item
                    {
                        Code = code,
                        Name = name,
                        Unit = string.IsNullOrWhiteSpace(unit) ? "عدد" : unit,
                        MinStock = Math.Max(0, minStock),
                        Stock = Math.Max(0, stock),
                        Price = Math.Max(0, price),
                        GroupName = group,
                        LocationName = location
                    });
                    imported++;
                }
                else
                {
                    existing.Name = name;
                    existing.Unit = string.IsNullOrWhiteSpace(unit) ? existing.Unit : unit;
                    existing.MinStock = Math.Max(0, minStock);
                    existing.Stock = Math.Max(0, stock);
                    existing.Price = Math.Max(0, price);
                    existing.GroupName = string.IsNullOrWhiteSpace(group) ? existing.GroupName : group;
                    existing.LocationName = string.IsNullOrWhiteSpace(location) ? existing.LocationName : location;
                    updated++;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"خط {i + 2}: {ex.Message}");
            }
        }

        Activities.Insert(0, new Activity
        {
            Type = "سیستم",
            ItemName = "ایمپورت Tbl_Product",
            Quantity = imported + updated,
            Party = $"جدید: {imported} | بروزرسانی: {updated}"
        });

        return (imported, updated, skipped, errors);
    }

    private int ApplyMovementTable(string? path, int sign, string activityType)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return 0;

        var rows = ReadCsv(path, out var headers);
        int idxCode = FindHeader(headers, "product_code", "code", "کد");
        int idxQty = FindHeader(headers, "qty", "quantity", "product_qty", "count", "تعداد");

        if (idxCode < 0 || idxQty < 0) return 0;

        var affected = 0;
        foreach (var row in rows)
        {
            var code = GetCol(row, idxCode);
            var qty = ParseInt(GetCol(row, idxQty));
            if (string.IsNullOrWhiteSpace(code) || qty <= 0) continue;

            var item = Items.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            if (item is null) continue;

            var delta = qty * sign;
            item.Stock = Math.Max(0, item.Stock + delta);
            affected += qty;
        }

        Activities.Insert(0, new Activity
        {
            Type = "سیستم",
            ItemName = activityType,
            Quantity = affected,
            Party = sign > 0 ? "افزایش موجودی" : "کاهش موجودی"
        });

        return affected;
    }

    private static List<string[]> ReadCsv(string path, out string[] headers)
    {
        var lines = File.ReadAllLines(path).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        if (lines.Length == 0)
        {
            headers = [];
            return [];
        }

        var separator = lines[0].Contains(';') ? ';' : ',';
        headers = lines[0].Split(separator).Select(NormalizeHeader).ToArray();
        return lines.Skip(1).Select(l => l.Split(separator)).ToList();
    }

    private static string NormalizeHeader(string value)
        => value.Trim().Trim('"').Replace(" ", string.Empty).ToLowerInvariant();

    private static int FindHeader(string[] headers, params string[] aliases)
        => Array.FindIndex(headers, h => aliases.Any(a => h.Contains(a, StringComparison.OrdinalIgnoreCase)));

    private static string GetCol(string[] cols, int idx)
        => idx >= 0 && idx < cols.Length ? cols[idx].Trim().Trim('"') : string.Empty;

    private static int ParseInt(string value)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var x)) return x;
        if (int.TryParse(value, out x)) return x;
        return 0;
    }

    private static decimal ParseDecimal(string value)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var x)) return x;
        if (decimal.TryParse(value, out x)) return x;
        return 0;
    }
}
