using System.Collections.Concurrent;
using DaricTrading.Domain;

namespace DaricTrading.Infrastructure.Persistence.InMemory;

/// <summary>رویدادها را در صف ConcurrentQueue ثبت می‌کند تا آزمون‌ها بتوانند dispatch را مشاهده کنند.</summary>
public sealed class InMemoryDomainEventDispatcher : IDomainEventDispatcher
{
    /// <summary>صف امن رویدادهایی که این پیاده‌سازی دریافت کرده است.</summary>
    private readonly ConcurrentQueue<IDomainEvent> _events = new();
    /// <summary>تصویر رویدادهای منتشرشده را برای مشاهده و بررسی آزمون‌ها برمی‌گرداند.</summary>
    public IReadOnlyCollection<IDomainEvent> PublishedEvents => _events.ToArray();
    /// <summary>هر رویداد ورودی را در ConcurrentQueue قرار می‌دهد و cancellation token را رعایت می‌کند.</summary>
    public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in events) _events.Enqueue(domainEvent);
        return Task.CompletedTask;
    }
}
