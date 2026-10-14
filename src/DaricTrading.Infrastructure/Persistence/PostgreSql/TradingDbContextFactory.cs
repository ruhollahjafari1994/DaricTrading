using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DaricTrading.Infrastructure.Persistence.PostgreSql;

/// <summary>تنظیمات اتصال محیط طراحی را می‌خواند و DbContext لازم برای فرمان‌های EF را می‌سازد.</summary>
public sealed class TradingDbContextFactory : IDesignTimeDbContextFactory<TradingDbContext>
{
    /// <summary>زمینه پایگاه داده را برای ابزارهای طراحی و migration می‌سازد.</summary>
    public TradingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TradingDbContext>()
            .UseNpgsql("Host=localhost;Database=daric;Username=daric;Password=daric")
            .Options;
        return new TradingDbContext(options);
    }
}
