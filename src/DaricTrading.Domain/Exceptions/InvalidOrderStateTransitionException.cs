namespace DaricTrading.Domain;

/// <summary>درخواست تغییری را رد می‌کند که برای وضعیت فعلی سفارش مجاز نیست.</summary>
public sealed class InvalidOrderStateTransitionException(string message) : DomainRuleViolationException(message);
