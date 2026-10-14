namespace DaricTrading.Application.Orders.Queries;

/// <summary>مالک سفارش و فیلترهای اختیاری tag، تاریخ و نوع معامله را برای جست‌وجو حمل می‌کند.</summary>
/// <param name="UserId">مالک سفارش‌هایی که باید جست‌وجو شوند.</param>
/// <param name="Tag">فیلتر اختیاری برای تطبیق مقدار دقیق برچسب.</param>
/// <param name="From">زمان آغاز شامل برای محدود کردن CreatedAt.</param>
/// <param name="To">زمان پایان شامل برای محدود کردن CreatedAt.</param>
/// <param name="Type">فیلتر اختیاری نوع سفارش؛ فقط Buy یا Sell پذیرفته می‌شود.</param>
public sealed record SearchOrdersQuery(Guid UserId, string? Tag = null, DateTimeOffset? From = null, DateTimeOffset? To = null, string? Type = null);
