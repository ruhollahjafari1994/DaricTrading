using System.Collections.Concurrent;
using DaricTrading.Domain;

namespace DaricTrading.Infrastructure;

public sealed class InMemoryGoldWalletRepository : IGoldWalletRepository
{
    private readonly ConcurrentDictionary<Guid, GoldWallet> _wallets = new();
    public Task<GoldWallet> GetOrCreateAsync(Guid userId, CancellationToken ct = default) => Task.FromResult(_wallets.GetOrAdd(userId, id => new GoldWallet(id)));
}
public sealed class InMemoryGoldOrderRepository : IGoldOrderRepository
{
    private readonly ConcurrentDictionary<Guid, GoldOrder> _orders = new();
    public Task AddAsync(GoldOrder order, CancellationToken ct = default) { if (!_orders.TryAdd(order.Id, order)) throw new InvalidOperationException("Duplicate order."); return Task.CompletedTask; }
    public Task<GoldOrder?> GetAsync(Guid id, CancellationToken ct = default) { _orders.TryGetValue(id, out var order); return Task.FromResult(order); }
    public Task<IReadOnlyList<GoldOrder>> SearchAsync(Guid userId, string? tag, DateTimeOffset? from, DateTimeOffset? to, OrderType? type, CancellationToken ct = default)
    {
        IReadOnlyList<GoldOrder> result = _orders.Values.Where(x => x.UserId == userId && x.Status == OrderStatus.Settled && (tag == null || x.Tags.Any(t => t.Value == tag)) && (!from.HasValue || x.CreatedAt >= from) && (!to.HasValue || x.CreatedAt <= to) && (!type.HasValue || x.Type == type)).OrderByDescending(x => x.CreatedAt).ToArray();
        return Task.FromResult(result);
    }
}
