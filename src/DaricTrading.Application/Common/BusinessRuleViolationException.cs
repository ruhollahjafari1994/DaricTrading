namespace DaricTrading.Application.Common;

/// <summary>شکست یک قاعده کاربردی را برای تبدیل به پاسخ خطای API حمل می‌کند.</summary>
public sealed class BusinessRuleViolationException(string message) : Exception(message);
