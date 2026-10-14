namespace DaricTrading.Domain;

/// <summary>قرارداد دسترسی به زمان جاری را برای قابل‌آزمون بودن برنامه فراهم می‌کند.</summary>
public interface IClock
{
    /// <summary>زمان UTC جاری را برای ثبت زمان سفارش و رویداد به handlerها می‌دهد.</summary>
    DateTimeOffset UtcNow { get; }
}
