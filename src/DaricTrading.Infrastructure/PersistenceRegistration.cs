using DaricTrading.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DaricTrading.Infrastructure;

public static class PersistenceRegistration
{
    public static IServiceCollection AddInMemoryTradingPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IGoldOrderRepository, InMemoryGoldOrderRepository>();
        services.AddSingleton<IGoldWalletRepository, InMemoryGoldWalletRepository>();
        services.AddSingleton<IDomainEventDispatcher, InMemoryDomainEventDispatcher>();
        services.AddSingleton<IClock, Persistence.PostgreSql.SystemClock>();
        return services;
    }

    public static async Task ApplyTradingMigrationsAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<Persistence.PostgreSql.TradingDbContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }
}


