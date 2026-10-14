namespace DaricTrading.Domain;

/// <summary>وقوع لغو یک سفارش را اعلام می‌کند.</summary>
/// <param name="OrderId">مقدار «OrderId» این قرارداد داده را مشخص می‌کند.</param>
/// <param name="UserId">مقدار «UserId» این قرارداد داده را مشخص می‌کند.</param>
/// <param name="OccurredAt">مقدار «OccurredAt» این قرارداد داده را مشخص می‌کند.</param>
public sealed record OrderCancelledDomainEvent(Guid OrderId, Guid UserId, DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);
