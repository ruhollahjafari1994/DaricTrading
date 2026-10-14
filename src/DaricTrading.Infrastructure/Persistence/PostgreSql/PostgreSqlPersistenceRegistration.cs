using DaricTrading.Application.Common;
using DaricTrading.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DaricTrading.Infrastructure.Persistence.PostgreSql;

/// <summary>ثبت وابستگی‌ها و راه‌اندازی schema مربوط به ذخیره‌سازی PostgreSQL را مدیریت می‌کند.</summary>
public static class PostgreSqlPersistenceRegistration
{
    /// <summary>زمینه داده، مخزن‌های PostgreSQL، واحد کار و سرویس زمان را ثبت می‌کند.</summary>
    public static IServiceCollection AddPostgreSqlPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TradingDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IGoldWalletRepository, PostgreSqlGoldWalletRepository>();
        services.AddScoped<IGoldOrderRepository, PostgreSqlGoldOrderRepository>();
        services.AddScoped<IDomainEventDispatcher, LoggingDomainEventDispatcher>();
        services.AddScoped<IUnitOfWork, PostgreSqlUnitOfWork>();
        services.AddSingleton<IClock, SystemClock>();
        return services;
    }

    /// <summary>migrationهای پایگاه داده معاملات را اجرا می‌کند.</summary>
    public static async Task ApplyTradingMigrationsAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }
}
