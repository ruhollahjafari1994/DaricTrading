namespace DaricTrading.Domain;

/// <summary>هنگامی پرتاب می‌شود که موجودی کیف پول برای وزن فروش کافی نیست.</summary>
public sealed class InsufficientGoldBalanceException() : DomainRuleViolationException("Insufficient gold balance.");
