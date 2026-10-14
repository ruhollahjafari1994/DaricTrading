namespace DaricTrading.Domain;

/// <summary>وقوع تسویه یک سفارش فروش طلا را اعلام می‌کند.</summary>
/// <param name="OrderId">مقدار «OrderId» این قرارداد داده را مشخص می‌کند.</param>
/// <param name="UserId">مقدار «UserId» این قرارداد داده را مشخص می‌کند.</param>
/// <param name="Weight">مقدار «Weight» این قرارداد داده را مشخص می‌کند.</param>
/// <param name="OccurredAt">مقدار «OccurredAt» این قرارداد داده را مشخص می‌کند.</param>
public sealed record GoldSoldDomainEvent(Guid OrderId, Guid UserId, GoldWeight Weight, DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);
