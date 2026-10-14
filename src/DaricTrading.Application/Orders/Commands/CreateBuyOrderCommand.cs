namespace DaricTrading.Application.Orders.Commands;

/// <summary>فرمان خرید را با شناسه کاربر، وزن، قیمت هر گرم، کارمزد و برچسب‌ها توصیف می‌کند.</summary>
/// <param name="UserId">شناسه کاربری که طلا برای او خریده می‌شود.</param>
/// <param name="WeightGrams">وزن خرید بر حسب گرم؛ دامنه دقت را تا سه رقم اعشار محدود می‌کند.</param>
/// <param name="UnitPrice">قیمت هر گرم طلا در لحظه سفارش.</param>
/// <param name="Fee">کارمزد محاسبه‌شده برای خرید.</param>
/// <param name="Tags">برچسب‌های اختیاری متصل به سفارش.</param>
public sealed record CreateBuyOrderCommand(
    Guid UserId,
    decimal WeightGrams,
    decimal UnitPrice,
    decimal Fee,
    IReadOnlyCollection<string>? Tags)
    : CreateOrderCommand(UserId, WeightGrams, UnitPrice, Fee, Tags);
