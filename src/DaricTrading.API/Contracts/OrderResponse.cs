namespace DaricTrading.API.Contracts;

/// <summary>نمایش عمومی سفارش در پاسخ HTTP که مستقل از انواع داخلی دامنه است.</summary>
/// <param name="Id">شناسه سفارش که برای خواندن یا تغییر metadata در URL استفاده می‌شود.</param>
/// <param name="UserId">شناسه مالک سفارش.</param>
/// <param name="Type">نوع معامله؛ Buy برای خرید و Sell برای فروش.</param>
/// <param name="WeightGrams">وزن معامله بر حسب گرم.</param>
/// <param name="UnitPrice">قیمت هر گرم در زمان سفارش.</param>
/// <param name="Fee">کارمزد منظورشده در مبلغ سفارش.</param>
/// <param name="TotalAmount">مبلغ نهایی سفارش با احتساب یا کسر کارمزد متناسب با نوع معامله.</param>
/// <param name="Currency">کد واحد پول مقادیر مالی.</param>
/// <param name="Tags">برچسب‌های متنی سفارش.</param>
/// <param name="UserNote">یادداشت اختیاری کاربر.</param>
/// <param name="Status">وضعیت فعلی سفارش.</param>
/// <param name="CreatedAt">زمان ایجاد سفارش به صورت DateTimeOffset.</param>
public sealed record OrderResponse(Guid Id, Guid UserId, string Type, decimal WeightGrams, decimal UnitPrice, decimal Fee, decimal TotalAmount, string Currency, IReadOnlyList<string> Tags, string? UserNote, string Status, DateTimeOffset CreatedAt)
{
    /// <summary>خروجی لایه کاربردی را به قرارداد پاسخ API تبدیل می‌کند.</summary>
    public static OrderResponse From(DaricTrading.Application.Orders.OrderDto order) => new(order.Id, order.UserId, order.Type, order.WeightGrams, order.UnitPrice, order.Fee, order.TotalAmount, order.Currency, order.Tags.ToArray(), order.Note, order.Status, order.CreatedAt);
}
