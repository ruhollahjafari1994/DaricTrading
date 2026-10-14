namespace DaricTrading.Domain;

/// <summary>موجودی طلای هر کاربر را نگه می‌دارد و افزایش یا کاهش امن آن را اعمال می‌کند.</summary>
public sealed class GoldWallet
{
    /// <summary>قفل هم‌زمانی برای هماهنگ‌کردن دسترسی‌های هم‌زمان به این شیء.</summary>
    private readonly object _gate = new();
    /// <summary>مقدار وزن فعلی کیف پول را نگه می‌دارد؛ تغییر آن فقط با Credit یا Debit انجام می‌شود.</summary>
    private GoldWeight _balance;
    /// <summary>نسخه داخلی aggregate را برای کنترل optimistic concurrency نگه می‌دارد.</summary>
    private long _version;

    /// <summary>کلید کیف پول و شناسه کاربری است که موجودی طلا به او تعلق دارد.</summary>
    public Guid UserId { get; }
    /// <summary>موجودی فعلی طلا در کیف پول.</summary>
    public GoldWeight Balance
    {
        get
        {
            lock (_gate)
            {
                return _balance;
            }
        }
    }

    /// <summary>شماره نسخه برای کنترل هم‌زمانی تغییرات.</summary>
    public long Version
    {
        get
        {
            lock (_gate)
            {
                return _version;
            }
        }
    }

    /// <summary>کیف پول کاربر را با موجودی اولیه صفر می‌سازد.</summary>
    public GoldWallet(Guid userId) : this(userId, GoldWeight.Zero, 0) { }
    /// <summary>کیف پول را با موجودی و نسخه خوانده‌شده از ذخیره‌ساز بازسازی می‌کند.</summary>
    private GoldWallet(Guid userId, GoldWeight balance, long version)
    {
        if (userId == Guid.Empty) throw new DomainRuleViolationException("Wallet user id is required.");
        UserId = userId;
        _balance = balance;
        _version = version;
    }
    /// <summary>موجودی و version persistence را اعتبارسنجی و کیف پول را با همان version بازسازی می‌کند.</summary>
    public static GoldWallet Restore(Guid userId, decimal balanceGrams, long version)
    {
        if (balanceGrams < 0 || decimal.Round(balanceGrams, 3) != balanceGrams || version < 0)
            throw new DomainRuleViolationException("Wallet balance must be non-negative with at most three decimal places.");
        return new GoldWallet(userId, balanceGrams == 0 ? GoldWeight.Zero : GoldWeight.FromGrams(balanceGrams), version);
    }
    /// <summary>زیر قفل، وزن را به موجودی اضافه و نسخه کیف پول را برای تشخیص concurrency زیاد می‌کند.</summary>
    public void Credit(GoldWeight amount)
    {
        ArgumentNullException.ThrowIfNull(amount);
        lock (_gate)
        {
            _balance = GoldWeight.FromGrams(_balance.Grams + amount.Grams);
            _version++;
        }
    }
    /// <summary>زیر قفل کفایت موجودی را می‌سنجد؛ در صورت کافی بودن وزن را کم و نسخه را زیاد می‌کند.</summary>
    public void Debit(GoldWeight amount)
    {
        ArgumentNullException.ThrowIfNull(amount);
        lock (_gate)
        {
            if (_balance.Grams < amount.Grams) throw new InsufficientGoldBalanceException();
            var remaining = _balance.Grams - amount.Grams;
            _balance = remaining == 0 ? GoldWeight.Zero : GoldWeight.FromGrams(remaining);
            _version++;
        }
    }
}
