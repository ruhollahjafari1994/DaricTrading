namespace DaricTrading.Domain;

/// <summary>قرارداد انتشار رویدادهای دامنه را تعریف می‌کند.</summary>
public interface IDomainEventDispatcher
{
    /// <summary>رویدادهای دامنه تولیدشده را پس از ذخیره aggregate به مقصد انتشار می‌فرستد.</summary>
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken cancellationToken = default);
}
