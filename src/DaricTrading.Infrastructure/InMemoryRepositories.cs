using System.Collections.Concurrent;
using DaricTrading.Domain;

namespace DaricTrading.Infrastructure;

public sealed class InMemoryGoldWalletRepository : IGoldWalletRepository
{
    private readonly ConcurrentDictionary<Guid, GoldWallet> _wallets = new();
    public Task<GoldWallet> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_wallets.GetOrAdd(userId, id => new GoldWallet(id)));
    public Task SaveAsync(GoldWallet wallet, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public sealed class InMemoryGoldOrderRepository : IGoldOrderRepository
{
    private readonly ConcurrentDictionary<Guid, GoldOrder> _orders = new();
    public Task AddAsync(GoldOrder order, CancellationToken cancellationToken = default)
    {
        if (!_orders.TryAdd(order.Id, order)) throw new InvalidOperationException("Duplicate order id.");
        return Task.CompletedTask;
    }
    public Task UpdateAsync(GoldOrder order, CancellationToken cancellationToken = default)
    {
        if (!_orders.ContainsKey(order.Id)) throw new KeyNotFoundException($"Order '{order.Id}' was not found.");
        _orders[order.Id] = order;
        return Task.CompletedTask;
    }
    public Task<GoldOrder?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _orders.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }
    public Task<IReadOnlyList<GoldOrder>> SearchAsync(Guid userId, string? tag, DateTimeOffset? from, DateTimeOffset? to, OrderType? type, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<GoldOrder> result = _orders.Values
            .Where(order => order.UserId == userId && order.Status == OrderStatus.Settled)
            .Where(order => tag is null || order.Tags.Any(orderTag => orderTag.Value == tag))
            .Where(order => !from.HasValue || order.CreatedAt >= from.Value)
            .Where(order => !to.HasValue || order.CreatedAt <= to.Value)
            .Where(order => !type.HasValue || order.Type == type.Value)
            .OrderByDescending(order => order.CreatedAt)
            .ToArray();
        return Task.FromResult(result);
    }
}

public sealed class InMemoryDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly ConcurrentQueue<IDomainEvent> _events = new();
    public IReadOnlyCollection<IDomainEvent> PublishedEvents => _events.ToArray();
    public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in events) _events.Enqueue(domainEvent);
        return Task.CompletedTask;
    }
}
