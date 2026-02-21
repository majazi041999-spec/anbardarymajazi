# چک‌لیست آماده‌سازی نسخه نهایی (EXE)

این چک‌لیست کمک می‌کند بدانیم چه زمانی می‌توان خروجی نهایی EXE را با اطمینان تحویل داد.

## 1) پیش‌نیازهای محیط ساخت
- نصب .NET SDK 8
- نصب Visual Studio 2022 با Workload «ASP.NET and web development»
- دسترسی به SQL Server (برای فاز Persistence)

## 2) پیش‌نیازهای فنی قبل از خروجی نهایی
- [ ] اتصال Store به EF Core/SQL Server (خروج از حالت in-memory)
- [ ] احراز هویت و نقش‌ها (انباردار/مدیر/مشاهده‌گر)
- [ ] ثبت Audit Log عملیات حساس
- [ ] مدیریت خطای استاندارد + لاگ‌گیری
- [ ] Seed داده اولیه برای راه‌اندازی سریع

## 3) تست‌های اجباری پیش از Release
- [ ] `dotnet restore`
- [ ] `dotnet build -c Release`
- [ ] `dotnet test -c Release` (در صورت وجود تست‌ها)
- [ ] Smoke Test API روی محیط لوکال
- [ ] بررسی سناریوهای ورود/خروج/گزارش در UI

## 4) ساخت EXE

### گزینه سریع (دستی)
```bash
dotnet publish src/Inventory.Api/Inventory.Api.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

### با اسکریپت
- PowerShell: `./scripts/publish-win-x64.ps1`
- Bash: `./scripts/publish-win-x64.sh`

## 5) تحویل نهایی
- [ ] پوشه `publish` خروجی گرفته شود.
- [ ] فایل راهنمای نصب برای کاربر اداری ضمیمه شود.
- [ ] نسخه‌بندی (مثلاً `v1.0.0`) روی Git ثبت شود.

---

## وضعیت فعلی پروژه
در حال حاضر پروژه در مرحله MVP است؛ بنابراین هنوز «نسخه نهایی قابل تحویل سازمانی» محسوب نمی‌شود.
زمانی که چک‌لیست بالا تکمیل شود، می‌توانیم خروجی نهایی EXE را به‌صورت رسمی تحویل دهیم.
