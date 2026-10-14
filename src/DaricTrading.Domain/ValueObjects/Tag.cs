namespace DaricTrading.Domain;

/// <summary>برچسب سفارش را نگه می‌دارد و مجاز بودن مقدار آن را کنترل می‌کند.</summary>
public sealed class Tag : IEquatable<Tag>
{
    /// <summary>مقدار داخلی «Allowed» برای اجرای این نوع.</summary>
    private static readonly HashSet<string> Allowed = new(StringComparer.Ordinal)
    {
        "پس‌انداز_بلندمدت",
        "نوسان‌گیری_روزانه",
        "صندوق_طلای_فیزیکی",
        "تسویه_شمش"
    };

    /// <summary>مقدار متنی برچسب.</summary>
    public string Value { get; }

    /// <summary>فاصله‌های ابتدا و انتها را حذف و خالی بودن، طول بیش از ۶۴ و عضویت در allowlist را رد می‌کند.</summary>
    public Tag(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleViolationException("Tag is empty, too long, or not allowed.");
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length == 0 || normalizedValue.Length > 64 || !Allowed.Contains(normalizedValue))
            throw new DomainRuleViolationException("Tag is empty, too long, or not allowed.");

        Value = normalizedValue;
    }
    /// <summary>برابری این مقدار را با مقدار دیگر بر اساس قواعد نوع بررسی می‌کند.</summary>
    public bool Equals(Tag? other) => other is not null && Value == other.Value;
    /// <summary>برابری این مقدار را با مقدار دیگر طبق قواعد نوع بررسی می‌کند.</summary>
    public override bool Equals(object? obj) => obj is Tag other && Equals(other);
    /// <summary>کد هش سازگار با قواعد برابری مقدار تولید می‌کند.</summary>
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    /// <summary>نمایش متنی این مقدار را برمی‌گرداند.</summary>
    public override string ToString() => Value;
}
