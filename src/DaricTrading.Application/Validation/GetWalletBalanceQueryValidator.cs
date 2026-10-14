using DaricTrading.Application.Wallets;
using FluentValidation;

namespace DaricTrading.Application.Validation;

/// <summary>شناسه کاربر را پیش از خواندن کیف پول الزامی می‌کند.</summary>
public sealed class GetWalletBalanceQueryValidator : AbstractValidator<GetWalletBalanceQuery>
{
    /// <summary>قانون ضروری بودن شناسه کاربر برای دریافت موجودی را ثبت می‌کند.</summary>
    public GetWalletBalanceQueryValidator()
    {
        RuleFor(query => query.UserId).NotEmpty();
    }
}
