namespace DaricTrading.Domain;

/// <summary>عملیات ذخیره‌سازی و جست‌وجوی سفارش‌ها را برای لایه دامنه تعریف می‌کند.</summary>
public interface IGoldOrderRepository
{
    /// <summary>سفارش تازه را به مخزن اضافه می‌کند تا در واحد کار جاری ذخیره شود.</summary>
    Task AddAsync(GoldOrder order, CancellationToken cancellationToken = default);
    /// <summary>سفارش را با شناسه‌اش پیدا می‌کند و در صورت نبودن null می‌دهد.</summary>
    Task<GoldOrder?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>تغییرات aggregate سفارش بازیابی‌شده را برای ذخیره ثبت می‌کند.</summary>
    Task UpdateAsync(GoldOrder order, CancellationToken cancellationToken = default);
    /// <summary>سفارش‌های تسویه‌شده کاربر را با برچسب، تاریخ و نوع معامله فیلتر می‌کند.</summary>
    Task<IReadOnlyList<GoldOrder>> SearchAsync(Guid userId, string? tag, DateTimeOffset? from, DateTimeOffset? to, OrderType? type, CancellationToken cancellationToken = default);
}
