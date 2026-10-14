namespace DaricTrading.Application.Common;

/// <summary>تداخل optimistic concurrency در ذخیره‌سازی را به لایه API منتقل می‌کند.</summary>
public sealed class OptimisticConcurrencyException(string message) : Exception(message);
