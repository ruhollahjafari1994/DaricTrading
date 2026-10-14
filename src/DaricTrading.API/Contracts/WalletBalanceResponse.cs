using DaricTrading.Application.Wallets;

namespace DaricTrading.API.Contracts;

/// <summary>قرارداد پاسخ API برای نمایش موجودی کیف پول طلای کاربر.</summary>
/// <param name="UserId">شناسه کاربری که موجودی پاسخ متعلق به اوست.</param>
/// <param name="BalanceGrams">وزن طلای موجود در کیف پول، به گرم.</param>
public sealed record WalletBalanceResponse(Guid UserId, decimal BalanceGrams)
{
    /// <summary>DTO لایه کاربردی را به پاسخ عمومی API تبدیل می‌کند.</summary>
    public static WalletBalanceResponse From(WalletBalanceDto balance) => new(balance.UserId, balance.BalanceGrams);
}
