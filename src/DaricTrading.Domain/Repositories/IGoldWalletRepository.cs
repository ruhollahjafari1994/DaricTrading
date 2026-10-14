namespace DaricTrading.Domain;

/// <summary>عملیات دسترسی و ذخیره کیف پول را برای لایه دامنه تعریف می‌کند.</summary>
public interface IGoldWalletRepository
{
    /// <summary>کیف پول ذخیره‌شده کاربر را بازیابی می‌کند یا در صورت نبود null می‌دهد.</summary>
    Task<GoldWallet?> GetAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>کیف پول موجود را می‌دهد و در صورت نیاز آن را به شکل امن ایجاد می‌کند.</summary>
    Task<GoldWallet> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>نسخه جاری کیف پول را برای ذخیره در واحد کار آماده می‌کند.</summary>
    Task SaveAsync(GoldWallet wallet, CancellationToken cancellationToken = default);
}
