using DaricTrading.Application.Common;
using DaricTrading.Domain;
using DaricTrading.Infrastructure.Persistence.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace DaricTrading.Infrastructure;

/// <summary>ثبت وابستگی‌های پیاده‌سازی ذخیره‌سازی حافظه‌ای را متمرکز می‌کند.</summary>
public static class InMemoryPersistenceRegistration
{
    /// <summary>مخزن‌های حافظه‌ای، انتشارگر رویداد و ساعت سیستم را در تزریق وابستگی ثبت می‌کند.</summary>
    public static IServiceCollection AddInMemoryTradingPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IGoldOrderRepository, InMemoryGoldOrderRepository>();
        services.AddSingleton<IGoldWalletRepository, InMemoryGoldWalletRepository>();
        services.AddSingleton<IDomainEventDispatcher, InMemoryDomainEventDispatcher>();
        services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();
        services.AddSingleton<IClock, Persistence.PostgreSql.SystemClock>();
        return services;
    }

}
