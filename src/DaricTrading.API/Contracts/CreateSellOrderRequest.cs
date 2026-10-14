namespace DaricTrading.API.Contracts;

/// <summary>اطلاعاتی که برای ثبت سفارش فروش طلا از طریق API دریافت می‌شود.</summary>
/// <param name="UserId">شناسه کاربری که طلای او از کیف پول کم می‌شود.</param>
/// <param name="WeightGrams">وزن فروش بر حسب گرم و با حداکثر سه رقم اعشار.</param>
/// <param name="UnitPrice">قیمت هر گرم طلا به واحد پول IRR.</param>
/// <param name="Fee">کارمزد فروش به واحد پول IRR.</param>
/// <param name="Tags">برچسب‌های اختیاری از فهرست مجاز سامانه.</param>
public sealed record CreateSellOrderRequest(Guid UserId, decimal WeightGrams, decimal UnitPrice, decimal Fee, string[]? Tags);
