# سامانه انبارداری اداری (نسخه جایگزین Access)

این مخزن برای طراحی و پیاده‌سازی یک سامانه انبارداری مدرن با **C# / .NET** ایجاد شده است تا جایگزین فایل Access فعلی شود.

## وضعیت فعلی
فاز برنامه‌ریزی انجام شده و **فاز 1 (MVP عملیاتی)** در حال توسعه است:
- اسکلت پروژه `ASP.NET Core 8 Minimal API`
- ماژول اولیه «تعریف کالا/اموال»
- ماژول «رسید انبار (ورود کالا)»
- ماژول «حواله خروج (تحویل به واحدها)» با کنترل موجودی ناکافی
- داشبورد خلاصه آماری برای گزارش سریع
- پیام‌های اعتبارسنجی فارسی برای کاربران غیر فنی

## ساختار پروژه
- `anbardarymajazi.sln`
- `src/Inventory.Api`
  - `Program.cs`
  - `Endpoints/*.cs`
  - `Domain/*.cs`
  - `Infrastructure/InventoryStore.cs`
  - `Contracts/*.cs`

## APIهای پیاده‌سازی‌شده (MVP)
- `GET /health`
- `GET /api/items`
- `POST /api/items`
- `GET /api/receipts/recent`
- `POST /api/receipts`
- `GET /api/issues/recent`
- `POST /api/issues`
- `GET /api/dashboard/summary`

## نکته مهم
در این محیط اجرایی، `dotnet` نصب نیست؛ بنابراین امکان Build/Run واقعی داخل همین کانتینر وجود نداشت.
اما ساختار پروژه و کدها به‌صورت استاندارد .NET 8 آماده شده‌اند.

## گام بعدی پیشنهادی
در مرحله بعدی (با تایید شما) انجام می‌دهم:
1. اتصال به SQL Server + EF Core Migration
2. احراز هویت نقش‌محور (انباردار/مدیر/مشاهده‌گر)
3. ثبت Audit Log برای عملیات حساس
4. ساخت UI با Blazor Server + داشبورد نموداری

## نقشه راه کامل
- [`docs/implementation-roadmap-fa.md`](docs/implementation-roadmap-fa.md)
