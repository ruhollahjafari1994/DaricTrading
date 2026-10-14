namespace DaricTrading.Domain;

/// <summary>قرارداد پایه رویدادهای دامنه را تعریف می‌کند.</summary>
public interface IDomainEvent
{
    /// <summary>لحظه UTC وقوع این رخداد در دامنه را مشخص می‌کند.</summary>
    DateTimeOffset OccurredAt { get; }
}

/// <summary>اطلاعات مشترک رویدادهای دامنه، از جمله زمان وقوع، را نگه می‌دارد.</summary>
/// <param name="OccurredAt">زمان UTC که رخداد در دامنه به وقوع پیوست.</param>
public abstract record DomainEvent(DateTimeOffset OccurredAt) : IDomainEvent;
