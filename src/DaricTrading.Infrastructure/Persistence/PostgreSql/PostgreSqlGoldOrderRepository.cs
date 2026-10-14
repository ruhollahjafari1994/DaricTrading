using DaricTrading.Domain;
using Microsoft.EntityFrameworkCore;

namespace DaricTrading.Infrastructure.Persistence.PostgreSql;

/// <summary>aggregate سفارش را به سند EF Core نگاشت کرده و آن را از جدول سفارش‌های PostgreSQL می‌خواند و می‌نویسد.</summary>
public sealed class PostgreSqlGoldOrderRepository(TradingDbContext dbContext) : IGoldOrderRepository
{
    /// <summary>OrderDocument را به DbContext اضافه می‌کند تا ثبت آن همراه واحد کار انجام شود.</summary>
    public Task AddAsync(GoldOrder order, CancellationToken cancellationToken = default)
    {
        dbContext.Orders.Add(OrderDocument.From(order));
        return Task.CompletedTask;
    }

    /// <summary>سند موجود و tracked را با وضعیت جاری aggregate همگام می‌کند.</summary>
    public async Task UpdateAsync(GoldOrder order, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.Orders.SingleOrDefaultAsync(item => item.Id == order.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Order '{order.Id}' was not found.");
        var nextVersion = document.Version + 1;
        var updated = OrderDocument.From(order, nextVersion);
        dbContext.Entry(document).CurrentValues.SetValues(updated);
        return;
    }

    /// <summary>OrderDocument را با کلید می‌خواند و در صورت وجود آن را به GoldOrder بازسازی می‌کند.</summary>
    public async Task<GoldOrder?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.Orders.SingleOrDefaultAsync(order => order.Id == id, cancellationToken);
        return document?.ToDomain();
    }

    /// <summary>query را در PostgreSQL براساس کاربر و Settled محدود کرده و فیلترهای tag/date/type را اعمال می‌کند.</summary>
    public async Task<IReadOnlyList<GoldOrder>> SearchAsync(Guid userId, string? tag, DateTimeOffset? from, DateTimeOffset? to, OrderType? type, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Orders.AsNoTracking().Where(order => order.UserId == userId && order.Status == (int)OrderStatus.Settled);
        if (from.HasValue) query = query.Where(order => order.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(order => order.CreatedAt <= to.Value);
        if (type.HasValue) query = query.Where(order => order.Type == (int)type.Value);
        var documents = await query.OrderByDescending(order => order.CreatedAt).ToListAsync(cancellationToken);
        IEnumerable<GoldOrder> results = documents.Select(document => document.ToDomain());
        if (tag is not null) results = results.Where(order => order.Tags.Any(orderTag => orderTag.Value == tag));
        return results.ToArray();
    }
}
