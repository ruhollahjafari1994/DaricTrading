namespace DaricTrading.Application.Orders.Commands;

/// <summary>فرمان فروش را با شناسه کاربر، وزن، قیمت هر گرم، کارمزد و برچسب‌ها توصیف می‌کند.</summary>
/// <param name="UserId">شناسه کاربری که طلای او فروخته می‌شود.</param>
/// <param name="WeightGrams">وزن فروش بر حسب گرم و حداکثر سه رقم اعشار.</param>
/// <param name="UnitPrice">قیمت هر گرم طلا در سفارش فروش.</param>
/// <param name="Fee">کارمزد کسرشده از مبلغ ناخالص فروش.</param>
/// <param name="Tags">برچسب‌های اختیاری متصل به سفارش.</param>
public sealed record CreateSellOrderCommand(
    Guid UserId,
    decimal WeightGrams,
    decimal UnitPrice,
    decimal Fee,
    IReadOnlyCollection<string>? Tags)
    : CreateOrderCommand(UserId, WeightGrams, UnitPrice, Fee, Tags);
