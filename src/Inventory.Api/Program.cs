using Inventory.Api.Endpoints;
using Inventory.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<InventoryStore>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapItemEndpoints();
app.MapReceiptEndpoints();
app.MapIssueEndpoints();
app.MapDashboardEndpoints();
app.MapMasterDataEndpoints();

app.Run();
