using DaricTrading.Domain;
using Microsoft.EntityFrameworkCore;

namespace DaricTrading.Infrastructure.Persistence.PostgreSql;

/// <summary>سند موجودی را در جدول کیف پول نگه می‌دارد و تغییرات tracked را برای commit آماده می‌کند.</summary>
public sealed class PostgreSqlGoldWalletRepository(TradingDbContext dbContext) : IGoldWalletRepository
{
    /// <summary>سند کیف پول را با userId می‌خواند و balance/version را به aggregate تبدیل می‌کند.</summary>
    public async Task<GoldWallet?> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.Wallets.AsNoTracking()
            .SingleOrDefaultAsync(wallet => wallet.UserId == userId, cancellationToken);

        return document is null
            ? null
            : GoldWallet.Restore(document.UserId, document.BalanceGrams, document.Version);
    }

    /// <summary>کیف پول موجود را می‌خواند یا aggregate اولیه را برای درج در همین DbContext می‌سازد.</summary>
    public async Task<GoldWallet> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.Wallets.SingleOrDefaultAsync(wallet => wallet.UserId == userId, cancellationToken);
        return document is null ? new GoldWallet(userId) : GoldWallet.Restore(document.UserId, document.BalanceGrams, document.Version);
    }

    /// <summary>اگر سند در DbContext tracked است آن را همگام می‌کند؛ در غیر این صورت درج جدید را stage می‌کند.</summary>
    public Task SaveAsync(GoldWallet wallet, CancellationToken cancellationToken = default)
    {
        var document = dbContext.Wallets.Local
            .SingleOrDefault(candidate => candidate.UserId == wallet.UserId);

        if (document is null)
        {
            dbContext.Wallets.Add(new WalletDocument { UserId = wallet.UserId, BalanceGrams = wallet.Balance.Grams, Version = wallet.Version });
            return Task.CompletedTask;
        }
        document.BalanceGrams = wallet.Balance.Grams;
        document.Version = wallet.Version;
        return Task.CompletedTask;
    }
}
