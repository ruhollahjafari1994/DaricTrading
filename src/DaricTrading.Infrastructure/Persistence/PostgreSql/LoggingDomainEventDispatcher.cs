using DaricTrading.Domain;
using Microsoft.Extensions.Logging;

namespace DaricTrading.Infrastructure.Persistence.PostgreSql;

/// <summary>شناسه سفارش، کاربر و زمان وقوع هر رویداد دامنه را با logger ثبت می‌کند.</summary>
public sealed class LoggingDomainEventDispatcher(ILogger<LoggingDomainEventDispatcher> logger) : IDomainEventDispatcher
{
    /// <summary>اطلاعات هر رویداد را به صورت structured log می‌نویسد؛ handlerها آن را پس از commit صدا می‌زنند.</summary>
    public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in events) logger.LogInformation("Domain event published: {EventType} at {OccurredAt}", domainEvent.GetType().Name, domainEvent.OccurredAt);
        return Task.CompletedTask;
    }
}
