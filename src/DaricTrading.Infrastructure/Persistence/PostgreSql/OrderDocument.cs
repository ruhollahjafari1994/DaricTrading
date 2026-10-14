using System.Text.Json;
using DaricTrading.Domain;

namespace DaricTrading.Infrastructure.Persistence.PostgreSql;

/// <summary>ساختار ذخیره‌سازی سفارش در پایگاه داده را نگه می‌دارد.</summary>
public sealed class OrderDocument
{
    /// <summary>شناسه یکتای این موجودیت یا سفارش.</summary>
    public Guid Id { get; set; }
    /// <summary>شناسه مالک سفارش که برای فیلتر و ایندکس سفارش‌های هر کاربر ذخیره می‌شود.</summary>
    public Guid UserId { get; set; }
    /// <summary>نوع عملیات سفارش، خرید یا فروش.</summary>
    public int Type { get; set; }
    /// <summary>وزن طلا بر حسب گرم.</summary>
    public decimal WeightGrams { get; set; }
    /// <summary>قیمت هر واحد وزن طلا.</summary>
    public decimal UnitPrice { get; set; }
    /// <summary>کد واحد پول مبلغ.</summary>
    public string Currency { get; set; } = "IRR";
    /// <summary>کارمزد محاسبه‌شده سفارش.</summary>
    public decimal Fee { get; set; }
    /// <summary>مبلغ نهایی سفارش پس از اعمال کارمزد.</summary>
    public decimal TotalAmount { get; set; }
    /// <summary>مقدار «TagsJson» را برای استفاده بخش‌های مرتبط نگه می‌دارد.</summary>
    public string TagsJson { get; set; } = "[]";
    /// <summary>وضعیت فعلی سفارش در چرخه عمر آن.</summary>
    public int Status { get; set; }
    /// <summary>یادداشت اختیاری کاربر درباره سفارش.</summary>
    public string? UserNote { get; set; }
    /// <summary>زمان ایجاد سفارش.</summary>
    public DateTimeOffset CreatedAt { get; set; }
    /// <summary>زمان تسویه سفارش، در صورت تسویه شدن.</summary>
    public DateTimeOffset? SettledAt { get; set; }
    /// <summary>شماره نسخه برای کنترل هم‌زمانی تغییرات.</summary>
    public long Version { get; set; }

    /// <summary>داده دامنه یا کاربردی را به ساختار مقصد تبدیل می‌کند.</summary>
    public static OrderDocument From(GoldOrder order, long version = 0) => new()
    {
        Id = order.Id,
        UserId = order.UserId,
        Type = (int)order.Type,
        WeightGrams = order.Weight.Grams,
        UnitPrice = order.UnitPrice.Amount,
        Currency = order.UnitPrice.Currency,
        Fee = order.Fee.Amount,
        TotalAmount = order.TotalAmount.Amount,
        TagsJson = JsonSerializer.Serialize(order.Tags.Select(tag => tag.Value)),
        Status = (int)order.Status,
        UserNote = order.UserNote,
        CreatedAt = order.CreatedAt,
        SettledAt = order.SettledAt,
        Version = version
    };

    /// <summary>سند ذخیره‌سازی را به موجودیت دامنه بازسازی می‌کند.</summary>
    public GoldOrder ToDomain()
    {
        var tagValues = JsonSerializer.Deserialize<string[]>(TagsJson)
            ?? throw new InvalidOperationException("Stored order tags are invalid.");

        var order = GoldOrder.Restore(
            Id,
            UserId,
            (OrderType)Type,
            GoldWeight.FromGrams(WeightGrams),
            new Money(UnitPrice, Currency),
            new Money(Fee, Currency),
            tagValues.Select(value => new Tag(value)),
            (OrderStatus)Status,
            UserNote,
            CreatedAt,
            SettledAt);

        if (order.TotalAmount.Amount != TotalAmount)
        {
            throw new InvalidOperationException("Stored order totals do not match the financial fields.");
        }

        return order;
    }
}
