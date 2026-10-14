using System.Collections.Concurrent;
using DaricTrading.Domain;

namespace DaricTrading.Infrastructure.Persistence.InMemory;

/// <summary>برای هر شناسه کاربر یک کیف پول thread-safe در ConcurrentDictionary نگه می‌دارد.</summary>
public sealed class InMemoryGoldWalletRepository : IGoldWalletRepository
{
    /// <summary>مخزن یا سرویس مورد استفاده برای دسترسی به کیف پول‌ها.</summary>
    private readonly ConcurrentDictionary<Guid, GoldWallet> _wallets = new();

    /// <summary>کیف پول را با شناسه کاربر از store هم‌زمان می‌خواند و در نبود آن null می‌دهد.</summary>
    public Task<GoldWallet?> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _wallets.TryGetValue(userId, out var wallet);
        return Task.FromResult(wallet);
    }

    /// <summary>GetOrAdd را برای جلوگیری از ساخت چند کیف پول هم‌زمان برای یک کاربر به کار می‌برد.</summary>
    public Task<GoldWallet> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_wallets.GetOrAdd(userId, id => new GoldWallet(id)));
    /// <summary>کار جداگانه‌ای ندارد چون repository همان مرجع aggregate را در store نگه داشته است.</summary>
    public Task SaveAsync(GoldWallet wallet, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
