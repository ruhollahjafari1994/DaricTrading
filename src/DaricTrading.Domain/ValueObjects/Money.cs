namespace DaricTrading.Domain;

/// <summary>مبلغ و واحد پول را با قواعد دقت و محاسبات مالی نمایش می‌دهد.</summary>
public sealed class Money : IEquatable<Money>
{
    /// <summary>سقف مبلغ سازگار با precision هجده رقم و دو رقم اعشار پایگاه داده را تعریف می‌کند.</summary>
    public const decimal MaximumSupportedAmount = 9_999_999_999_999_999.99m;
    /// <summary>کدهای پولی پذیرفته‌شده در این نسخه از پلتفرم را فهرست می‌کند.</summary>
    private static readonly HashSet<string> SupportedCurrencies = ["IRR", "USD", "EUR", "GBP"];

    /// <summary>مقدار عددی مبلغ.</summary>
    public decimal Amount { get; }
    /// <summary>کد واحد پول مبلغ.</summary>
    public string Currency { get; }
    /// <summary>مبلغ و کد پول را از نظر مقدار، دقت و پشتیبانی اعتبارسنجی می‌کند.</summary>
    public Money(decimal amount, string currency = "IRR")
    {
        if (amount < 0 || amount > MaximumSupportedAmount || decimal.Round(amount, 2) != amount)
            throw new DomainRuleViolationException("Money must fit a non-negative 18-digit amount with at most two decimal places.");
        if (string.IsNullOrWhiteSpace(currency) || !SupportedCurrencies.Contains(currency))
            throw new DomainRuleViolationException("Currency must be a supported ISO code: IRR, USD, EUR, or GBP.");
        Amount = amount;
        Currency = currency;
    }
    /// <summary>برابری این مقدار را با مقدار دیگر بر اساس قواعد نوع بررسی می‌کند.</summary>
    public bool Equals(Money? other) => other is not null && Amount == other.Amount && Currency == other.Currency;

    /// <summary>واحد پول را تطبیق می‌دهد و جمع را از سازنده Money عبور می‌دهد تا precision و محدوده حفظ شود.</summary>
    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>واحد پول را تطبیق می‌دهد و نتیجه را طوری می‌سازد که مبلغ منفی پذیرفته نشود.</summary>
    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    /// <summary>null بودن ورودی و متفاوت بودن Currency را پیش از arithmetic به خطای دامنه تبدیل می‌کند.</summary>
    private void EnsureSameCurrency(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Currency != other.Currency)
        {
            throw new DomainRuleViolationException("Money values must use the same currency.");
        }
    }

    /// <summary>واحد پول را تطبیق می‌دهد و جمع را از سازنده Money عبور می‌دهد تا precision و محدوده حفظ شود.</summary>
    public static Money operator +(Money left, Money right) => left.Add(right);

    /// <summary>واحد پول را تطبیق می‌دهد و نتیجه را طوری می‌سازد که مبلغ منفی پذیرفته نشود.</summary>
    public static Money operator -(Money left, Money right) => left.Subtract(right);

    /// <summary>برابری این مقدار را با مقدار دیگر بر اساس قواعد نوع بررسی می‌کند.</summary>
    public override bool Equals(object? obj) => obj is Money other && Equals(other);
    /// <summary>کد هش سازگار با قواعد برابری مقدار تولید می‌کند.</summary>
    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
}
