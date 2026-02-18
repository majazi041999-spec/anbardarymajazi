# سامانه انبارداری اداری (نسخه جایگزین Access)

این مخزن برای طراحی و پیاده‌سازی یک سامانه انبارداری مدرن با **C# / .NET** ایجاد شده است تا جایگزین فایل Access فعلی شود.

## وضعیت فعلی
پروژه در وضعیت **MVP پیشرفته** است (نه نسخه نهایی سازمانی):
- اسکلت پروژه `ASP.NET Core 8 Minimal API`
- ماژول «تعریف کالا/اموال»
- ماژول «رسید انبار (ورود کالا)»
- ماژول «حواله خروج (تحویل به واحدها)» با کنترل موجودی ناکافی
- داشبورد خلاصه آماری + روند زمانی + اقلام بحرانی + فعالیت‌های اخیر
- UI اولیه با طراحی **Glassmorphism** در مسیر `ui/` (RTL فارسی + UX ساده)
- امکان تنظیم آدرس API از داخل UI بدون نیاز به ویرایش کد
- مدیریت داده‌های پایه (تامین‌کننده/واحد) از داخل UI

## ساختار پروژه
- `anbardarymajazi.sln`
- `src/Inventory.Api`
  - `Program.cs`
  - `Endpoints/*.cs`
  - `Domain/*.cs`
  - `Infrastructure/InventoryStore.cs`
  - `Contracts/*.cs`
- `ui/`
  - `index.html`
  - `styles.css`
  - `app.js`
- `scripts/`
  - `publish-win-x64.ps1`
  - `publish-win-x64.sh`
- `docs/release-checklist-fa.md`

## APIهای پیاده‌سازی‌شده (MVP)
- `GET /health`
- `GET /api/items`
- `GET /api/items/low-stock`
- `GET /api/items/{itemId}/movements?take=50`
- `POST /api/items`
- `GET /api/receipts/recent`
- `POST /api/receipts`
- `GET /api/issues/recent`
- `POST /api/issues`
- `GET /api/dashboard/summary`
- `GET /api/dashboard/trend?days=14`
- `GET /api/dashboard/activity?take=20`
- `GET /api/masters/suppliers`
- `POST /api/masters/suppliers`
- `GET /api/masters/departments`
- `POST /api/masters/departments`

## پیش‌نیازها (برای اجرا بعد از Clone)
1. **Visual Studio 2022** (ترجیحاً 17.8 به بالا)
2. Workload: **ASP.NET and web development**
3. **.NET 8 SDK**
4. (اختیاری در این فاز) SQL Server — فعلاً Store درون‌حافظه‌ای است.

## اجرای پروژه
### اجرای API
```bash
dotnet restore
dotnet run --project src/Inventory.Api/Inventory.Api.csproj
```

### اجرای UI Glassmorphism
فایل `ui/index.html` را در مرورگر باز کنید.

> نکته: از داخل خود UI می‌توانید آدرس API را ذخیره کنید. پیش‌فرض `http://localhost:5000` است.


## اجرای سریع با Docker (بدون نصب مستقیم .NET روی سیستم)
```bash
docker build -t inventory-api .
docker run --rm -p 5000:5000 inventory-api
```

> سپس UI را باز کنید و آدرس API را روی `http://localhost:5000` بگذارید.

## ساخت EXE (Self-contained)
### روش مستقیم
```bash
dotnet publish src/Inventory.Api/Inventory.Api.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

### روش اسکریپتی
```powershell
./scripts/publish-win-x64.ps1
```
یا:
```bash
./scripts/publish-win-x64.sh
```

خروجی در مسیر `src/Inventory.Api/bin/Release/net8.0/win-x64/publish/` قرار می‌گیرد.

## نکته مهم درباره نسخه نهایی
برای اینکه خروجی EXE نهایی سازمانی تحویل شود، چک‌لیست این مسیر باید تکمیل گردد:
- [`docs/release-checklist-fa.md`](docs/release-checklist-fa.md)

## نقشه راه کامل
- [`docs/implementation-roadmap-fa.md`](docs/implementation-roadmap-fa.md)
