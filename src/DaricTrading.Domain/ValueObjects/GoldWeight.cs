namespace DaricTrading.Domain;

/// <summary>وزن طلا بر حسب گرم را با دقت و دامنه مجاز نگه می‌دارد.</summary>
public sealed record GoldWeight : IComparable<GoldWeight>
{
    /// <summary>بیشترین وزن قابل ذخیره با precision هجده رقم و سه رقم اعشار را تعریف می‌کند.</summary>
    public const decimal MaximumSupportedGrams = 999_999_999_999_999.999m;

    /// <summary>مقدار وزن بر حسب گرم.</summary>
    public decimal Grams { get; }

    /// <summary>وزن از پیش اعتبارسنجی‌شده را در شیء نگه می‌دارد.</summary>
    private GoldWeight(decimal grams) => Grams = grams;
    /// <summary>مثبت بودن، سقف عدد ذخیره‌پذیر و حداکثر سه رقم اعشار را پیش از ساخت وزن بررسی می‌کند.</summary>
    public static GoldWeight FromGrams(decimal grams)
    {
        if (grams <= 0 || grams > MaximumSupportedGrams || decimal.Round(grams, 3) != grams)
            throw new DomainRuleViolationException("Gold weight must be positive, fit an 18-digit amount, and have at most three decimal places.");
        return new GoldWeight(grams);
    }
    /// <summary>وزن صفر را برای مقداردهی اولیه کیف پول در اختیار دامنه قرار می‌دهد.</summary>
    internal static GoldWeight Zero => new(0m);
    /// <summary>گرم‌ها را جمع و خروجی را دوباره از FromGrams اعتبارسنجی می‌کند.</summary>
    public GoldWeight Add(GoldWeight other) => FromGrams(Grams + other.Grams);
    /// <summary>گرم‌ها را کم می‌کند؛ FromGrams حاصل صفر یا منفی را رد می‌کند.</summary>
    public GoldWeight Subtract(GoldWeight other) => FromGrams(Grams - other.Grams);
    /// <summary>این مقدار را با مقدار دیگر مقایسه می‌کند.</summary>
    public int CompareTo(GoldWeight? other) => other is null ? 1 : Grams.CompareTo(other.Grams);
    /// <summary>گرم‌ها را جمع و خروجی را دوباره از FromGrams اعتبارسنجی می‌کند.</summary>
    public static GoldWeight operator +(GoldWeight left, GoldWeight right) => left.Add(right);
    /// <summary>گرم‌ها را کم می‌کند؛ FromGrams حاصل صفر یا منفی را رد می‌کند.</summary>
    public static GoldWeight operator -(GoldWeight left, GoldWeight right) => left.Subtract(right);
    /// <summary>بررسی می‌کند وزن سمت چپ بیشتر از وزن سمت راست باشد.</summary>
    public static bool operator >(GoldWeight left, GoldWeight right) => left.Grams > right.Grams;
    /// <summary>بررسی می‌کند وزن سمت چپ کمتر از وزن سمت راست باشد.</summary>
    public static bool operator <(GoldWeight left, GoldWeight right) => left.Grams < right.Grams;
    /// <summary>بررسی می‌کند وزن سمت چپ حداقل برابر وزن سمت راست باشد.</summary>
    public static bool operator >=(GoldWeight left, GoldWeight right) => left.Grams >= right.Grams;
    /// <summary>بررسی می‌کند وزن سمت چپ حداکثر برابر وزن سمت راست باشد.</summary>
    public static bool operator <=(GoldWeight left, GoldWeight right) => left.Grams <= right.Grams;
}
