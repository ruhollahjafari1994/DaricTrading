using DaricTrading.Application.Common;
using DaricTrading.Domain;
using FluentValidation;

namespace DaricTrading.Application.Wallets;

/// <summary>موردکار خواندن موجودی طلای کاربر را اجرا می‌کند.</summary>
public sealed class GetWalletBalanceHandler(
    IGoldWalletRepository wallets,
    IValidator<GetWalletBalanceQuery> validator)
{
    /// <summary>شناسه کاربر را validate، کیف پول را بدون ایجاد کیف پول تازه می‌خواند و موجودی را DTO می‌کند.</summary>
    public async Task<WalletBalanceDto> HandleAsync(
        GetWalletBalanceQuery query,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);

        var wallet = await wallets.GetAsync(query.UserId, cancellationToken)
            ?? throw new NotFoundException("Wallet", query.UserId);

        return new WalletBalanceDto(wallet.UserId, wallet.Balance.Grams);
    }
}
