namespace DaricTrading.Application.Orders.Commands;

/// <summary>شناسه سفارش، مجموعه برچسب جایگزین و یادداشت جدید را برای به‌روزرسانی حمل می‌کند.</summary>
/// <param name="OrderId">شناسه سفارش دارای metadata قابل ویرایش.</param>
/// <param name="Tags">فهرست جایگزین؛ همه برچسب‌های قبلی با این مجموعه عوض می‌شوند.</param>
/// <param name="Note">یادداشت جدید که مقدار null برای حذف آن به کار می‌رود.</param>
public sealed record UpdateOrderTagsCommand(Guid OrderId, IReadOnlyCollection<string> Tags, string? Note);
