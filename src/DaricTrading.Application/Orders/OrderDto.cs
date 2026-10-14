using DaricTrading.Domain;

namespace DaricTrading.Application.Orders;

/// <summary>نتیجه خواندن یا ثبت سفارش را برای اپلیکیشن و mapperهای API منتقل می‌کند؛ به enumهای دامنه وابسته نیست.</summary>
/// <param name="Id">شناسه سفارش ذخیره‌شده.</param>
/// <param name="UserId">شناسه کاربری که سفارش به او تعلق دارد.</param>
/// <param name="Type">نوع سفارش به صورت متن Buy یا Sell.</param>
/// <param name="WeightGrams">وزن معامله بر حسب گرم.</param>
/// <param name="UnitPrice">قیمت هر گرم بر حسب واحد Currency.</param>
/// <param name="Fee">مقدار کارمزد سفارش.</param>
/// <param name="TotalAmount">مبلغ نهایی خرید یا دریافتی فروش پس از کارمزد.</param>
/// <param name="Currency">کد واحد پول قیمت و مبلغ نهایی.</param>
/// <param name="Tags">مقادیر متنی برچسب‌های سفارش.</param>
/// <param name="Note">یادداشت اختیاری کاربر درباره سفارش.</param>
/// <param name="Status">وضعیت سفارش به صورت متن.</param>
/// <param name="CreatedAt">زمان ثبت سفارش در UTC.</param>
public sealed record OrderDto(Guid Id, Guid UserId, string Type, decimal WeightGrams, decimal UnitPrice, decimal Fee, decimal TotalAmount, string Currency, IReadOnlyCollection<string> Tags, string? Note, string Status, DateTimeOffset CreatedAt)
{
    /// <summary>مقادیر aggregate را به داده‌های انتقالی تبدیل می‌کند و Value Objectها و enumها را به مقادیر ساده نگاشت می‌دهد.</summary>
    public static OrderDto From(GoldOrder order) => new(order.Id, order.UserId, order.Type.ToString(), order.Weight.Grams, order.UnitPrice.Amount, order.Fee.Amount, order.TotalAmount.Amount, order.TotalAmount.Currency, order.Tags.Select(tag => tag.Value).ToArray(), order.UserNote, order.Status.ToString(), order.CreatedAt);
}
