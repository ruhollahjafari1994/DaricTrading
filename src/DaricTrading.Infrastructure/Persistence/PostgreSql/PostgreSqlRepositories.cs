using DaricTrading.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace DaricTrading.Infrastructure.Persistence.PostgreSql;

public sealed class PostgreSqlGoldWalletRepository(TradingDbContext dbContext) : IGoldWalletRepository
{
    public async Task<GoldWallet> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.Wallets.SingleOrDefaultAsync(wallet => wallet.UserId == userId, cancellationToken);
        return document is null ? new GoldWallet(userId) : GoldWallet.Restore(document.UserId, document.BalanceGrams, document.Version);
    }

    public async Task SaveAsync(GoldWallet wallet, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.Wallets.FindAsync([wallet.UserId], cancellationToken);
        if (document is null)
        {
            dbContext.Wallets.Add(new WalletDocument { UserId = wallet.UserId, BalanceGrams = wallet.Balance.Grams, Version = wallet.Version });
            return;
        }
        document.BalanceGrams = wallet.Balance.Grams;
        document.Version = wallet.Version;
    }
}

public sealed class PostgreSqlGoldOrderRepository(TradingDbContext dbContext) : IGoldOrderRepository
{
    public async Task AddAsync(GoldOrder order, CancellationToken cancellationToken = default)
    {
        dbContext.Orders.Add(OrderDocument.From(order));
        await SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(GoldOrder order, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.Orders.SingleOrDefaultAsync(item => item.Id == order.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Order '{order.Id}' was not found.");
        var nextVersion = document.Version + 1;
        var updated = OrderDocument.From(order, nextVersion);
        dbContext.Entry(document).CurrentValues.SetValues(updated);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<GoldOrder?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.Orders.SingleOrDefaultAsync(order => order.Id == id, cancellationToken);
        return document?.ToDomain();
    }

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

    private async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try { await dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyConflictException("The record changed during this operation. Reload it and retry.");
        }
    }
}

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public sealed class LoggingDomainEventDispatcher(ILogger<LoggingDomainEventDispatcher> logger) : IDomainEventDispatcher
{
    public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in events) logger.LogInformation("Domain event published: {EventType} at {OccurredAt}", domainEvent.GetType().Name, domainEvent.OccurredAt);
        return Task.CompletedTask;
    }
}

public static class PostgreSqlPersistenceRegistration
{
    public static IServiceCollection AddPostgreSqlPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TradingDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IGoldWalletRepository, PostgreSqlGoldWalletRepository>();
        services.AddScoped<IGoldOrderRepository, PostgreSqlGoldOrderRepository>();
        services.AddScoped<IDomainEventDispatcher, LoggingDomainEventDispatcher>();
        services.AddSingleton<IClock, SystemClock>();
        return services;
    }
}



