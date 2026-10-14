namespace DaricTrading.API.Contracts;

/// <summary>فیلترهای جست‌وجوی سفارش؛ بازه تاریخ شامل ابتدا و انتها است و با قالب ISO 8601 ارسال می‌شود.</summary>
/// <param name="UserId">شناسه کاربری که سفارش‌هایش باید جست‌وجو شود.</param>
/// <param name="Tag">فیلتر اختیاری برای یافتن سفارش‌های دارای همین برچسب.</param>
/// <param name="From">آغاز شامل بازه زمانی ایجاد سفارش.</param>
/// <param name="To">پایان شامل بازه زمانی ایجاد سفارش.</param>
/// <param name="OrderType">فیلتر اختیاری نوع معامله با مقدار Buy یا Sell.</param>
public sealed record SearchOrdersQueryParameters(Guid UserId, string? Tag, DateTimeOffset? From, DateTimeOffset? To, string? OrderType);
