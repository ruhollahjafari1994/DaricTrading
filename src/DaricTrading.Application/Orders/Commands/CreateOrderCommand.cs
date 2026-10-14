namespace DaricTrading.Application.Orders.Commands;

/// <summary>فیلدهای مشترک مالی و شناسایی را برای فرمان‌های خرید و فروش فراهم می‌کند.</summary>
/// <param name="UserId">شناسه مالک سفارش.</param>
/// <param name="WeightGrams">وزن معامله بر حسب گرم.</param>
/// <param name="UnitPrice">قیمت یک گرم طلا.</param>
/// <param name="Fee">کارمزد معامله.</param>
/// <param name="Tags">برچسب‌های اختیاری سفارش.</param>
public abstract record CreateOrderCommand(
    Guid UserId,
    decimal WeightGrams,
    decimal UnitPrice,
    decimal Fee,
    IReadOnlyCollection<string>? Tags);
