using System.Collections.Concurrent;
using DaricTrading.Domain;

namespace DaricTrading.Infrastructure.Persistence.InMemory;

/// <summary>aggregate سفارش را در دیکشنری thread-safe نگه می‌دارد و عملیات جست‌وجوی MVP را اجرا می‌کند.</summary>
public sealed class InMemoryGoldOrderRepository : IGoldOrderRepository
{
    /// <summary>مخزن یا سرویس مورد استفاده برای دسترسی به سفارش‌ها.</summary>
    private readonly ConcurrentDictionary<Guid, GoldOrder> _orders = new();
    /// <summary>سفارش را با شناسه‌اش به صورت اتمیک اضافه می‌کند و شناسه تکراری را رد می‌کند.</summary>
    public Task AddAsync(GoldOrder order, CancellationToken cancellationToken = default)
    {
        if (!_orders.TryAdd(order.Id, order)) throw new InvalidOperationException("Duplicate order id.");
        return Task.CompletedTask;
    }
    /// <summary>نسخه موجود سفارش را با مقدار جدید جایگزین می‌کند و نبودن شناسه را گزارش می‌دهد.</summary>
    public Task UpdateAsync(GoldOrder order, CancellationToken cancellationToken = default)
    {
        if (!_orders.ContainsKey(order.Id)) throw new KeyNotFoundException($"Order '{order.Id}' was not found.");
        _orders[order.Id] = order;
        return Task.CompletedTask;
    }
    /// <summary>سفارش را از ConcurrentDictionary می‌خواند و برای شناسه ناشناخته null می‌دهد.</summary>
    public Task<GoldOrder?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _orders.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }
    /// <summary>سفارش‌های کاربر را با وضعیت تسویه، برچسب، تاریخ و نوع فیلتر می‌کند.</summary>
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
