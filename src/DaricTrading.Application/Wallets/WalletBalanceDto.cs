using DaricTrading.Domain;
using FluentValidation;

namespace DaricTrading.Application.Wallets;

/// <summary>شناسه کاربر و مقدار موجودی طلا را برای لایه کاربردی نگه می‌دارد.</summary>
/// <param name="UserId">شناسه کاربری که این موجودی به او تعلق دارد.</param>
/// <param name="BalanceGrams">موجودی طلای کاربر بر حسب گرم.</param>
public sealed record WalletBalanceDto(Guid UserId, decimal BalanceGrams);
