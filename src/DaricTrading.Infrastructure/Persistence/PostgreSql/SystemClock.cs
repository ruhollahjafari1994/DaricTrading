using DaricTrading.Domain;

namespace DaricTrading.Infrastructure.Persistence.PostgreSql;

/// <summary>زمان UTC جاری را از طریق قرارداد ساعت دامنه فراهم می‌کند.</summary>
public sealed class SystemClock : IClock
{
    /// <summary>زمان ثابت آزمون را از طریق قرارداد ساعت دامنه برمی‌گرداند.</summary>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
