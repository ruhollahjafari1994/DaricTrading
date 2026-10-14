namespace DaricTrading.Application.Common;

/// <summary>نوع منبع و شناسه پیدا نشده را برای پاسخ ۴۰۴ نگه می‌دارد.</summary>
public sealed class NotFoundException(string resource, Guid id) : Exception($"{resource} '{id}' was not found.");
