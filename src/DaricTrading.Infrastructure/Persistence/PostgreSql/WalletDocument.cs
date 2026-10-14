namespace DaricTrading.Infrastructure.Persistence.PostgreSql;

/// <summary>ساختار ذخیره‌سازی کیف پول در پایگاه داده را نگه می‌دارد.</summary>
public sealed class WalletDocument
{
    /// <summary>کلید اصلی ردیف کیف پول و شناسه کاربر مالک موجودی.</summary>
    public Guid UserId { get; set; }
    /// <summary>موجودی طلا بر حسب گرم.</summary>
    public decimal BalanceGrams { get; set; }
    /// <summary>شماره نسخه برای کنترل هم‌زمانی تغییرات.</summary>
    public long Version { get; set; }
}
