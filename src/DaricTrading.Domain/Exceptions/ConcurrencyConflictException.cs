namespace DaricTrading.Domain;

/// <summary>شکست اعمال تغییر به دلیل تداخل نسخه‌های هم‌زمان ذخیره‌سازی را نشان می‌دهد.</summary>
public sealed class ConcurrencyConflictException(string message) : DomainRuleViolationException(message);
