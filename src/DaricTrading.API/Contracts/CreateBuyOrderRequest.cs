namespace DaricTrading.API.Contracts;

/// <summary>اطلاعاتی که برای ثبت سفارش خرید طلا از طریق API دریافت می‌شود.</summary>
/// <param name="UserId">شناسه کاربری که سفارش خرید برای او ثبت می‌شود.</param>
/// <param name="WeightGrams">وزن خرید بر حسب گرم و با حداکثر سه رقم اعشار.</param>
/// <param name="UnitPrice">قیمت هر گرم طلا به واحد پول IRR.</param>
/// <param name="Fee">کارمزد خرید به واحد پول IRR.</param>
/// <param name="Tags">برچسب‌های اختیاری از فهرست مجاز سامانه.</param>
public sealed record CreateBuyOrderRequest(Guid UserId, decimal WeightGrams, decimal UnitPrice, decimal Fee, string[]? Tags);
