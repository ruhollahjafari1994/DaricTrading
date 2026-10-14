namespace DaricTrading.Domain;

/// <summary>خطای پایه‌ای است که aggregate هنگام نقض invariant دامنه پرتاب می‌کند.</summary>
public class DomainRuleViolationException(string message) : Exception(message);
