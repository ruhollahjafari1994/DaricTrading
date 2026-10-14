namespace DaricTrading.Application.Wallets;

/// <summary>شناسه کاربری را که باید موجودی کیف پولش خوانده شود حمل می‌کند.</summary>
/// <param name="UserId">شناسه کیف پولی که باید موجودی آن خوانده شود.</param>
public sealed record GetWalletBalanceQuery(Guid UserId);
