namespace DaricTrading.API.Contracts;

/// <summary>برچسب‌های جایگزین و یادداشت اختیاری برای به‌روزرسانی سفارش.</summary>
/// <param name="Tags">فهرست کامل برچسب‌های جدید؛ برچسب‌های پیشین با این فهرست جایگزین می‌شوند.</param>
/// <param name="Note">یادداشت جدید سفارش؛ null برای پاک کردن یادداشت به کار می‌رود.</param>
public sealed record UpdateOrderTagsRequest(string[] Tags, string? Note);
