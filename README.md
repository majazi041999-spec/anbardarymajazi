# سامانه انبارداری اداری (نسخه جایگزین Access)

این مخزن برای طراحی و پیاده‌سازی یک سامانه انبارداری مدرن با **C# / .NET** ایجاد شده است تا جایگزین فایل Access فعلی شود.

## وضعیت فعلی
فاز برنامه‌ریزی انجام شده و **شروع فاز 1 (MVP عملیاتی)** نیز در این مخزن پیاده‌سازی شده است:
- اسکلت پروژه `ASP.NET Core 8 Minimal API`
- ماژول اولیه «تعریف کالا/اموال»
- ماژول اولیه «رسید انبار (ورود کالا)»
- APIهای ساده و قابل توسعه با پیام‌های اعتبارسنجی فارسی

## ساختار پروژه
- `anbardarymajazi.sln`
- `src/Inventory.Api`
  - `Program.cs`
  - `Endpoints/ItemEndpoints.cs`
  - `Endpoints/ReceiptEndpoints.cs`
  - `Domain/Item.cs`
  - `Domain/StockReceipt.cs`
  - `Infrastructure/InventoryStore.cs`
  - `Contracts/*`

## APIهای پیاده‌سازی‌شده (MVP)
- `GET /health`
- `GET /api/items`
- `POST /api/items`
- `GET /api/receipts/recent`
- `POST /api/receipts`

## نکته مهم
در این محیط اجرایی، `dotnet` نصب نیست؛ بنابراین امکان Build/Run واقعی داخل همین کانتینر وجود نداشت.
اما ساختار پروژه و کدها به‌صورت استاندارد .NET 8 آماده شده‌اند.

## گام بعدی پیشنهادی
در مرحله بعدی (با تایید شما) انجام می‌دهم:
1. اتصال به SQL Server + EF Core Migration
2. افزودن حواله خروج + کنترل موجودی منفی
3. احراز هویت نقش‌محور (انباردار/مدیر/مشاهده‌گر)
4. صفحه داشبورد وب (Blazor) با نمودارهای داینامیک

## نقشه راه کامل
- [`docs/implementation-roadmap-fa.md`](docs/implementation-roadmap-fa.md)
