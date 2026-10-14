using Microsoft.EntityFrameworkCore;

namespace DaricTrading.Infrastructure.Persistence.PostgreSql;

/// <summary>ارتباط مدل برنامه با ذخیره‌سازی و نگاشت داده‌ها را مدیریت می‌کند.</summary>
public sealed class TradingDbContext(DbContextOptions<TradingDbContext> options) : DbContext(options)
{
    /// <summary>مجموعه سفارش‌ها را برای پرس‌وجو و ذخیره با EF Core فراهم می‌کند.</summary>
    public DbSet<OrderDocument> Orders => Set<OrderDocument>();
    /// <summary>مجموعه کیف پول‌ها را برای پرس‌وجو و ذخیره با EF Core فراهم می‌کند.</summary>
    public DbSet<WalletDocument> Wallets => Set<WalletDocument>();

    /// <summary>نگاشت موجودیت‌ها و قواعد مدل پایگاه داده را پیکربندی می‌کند.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderDocument>(entity =>
        {
            entity.ToTable("gold_orders");
            entity.HasKey(order => order.Id);
            entity.Property(order => order.WeightGrams).HasPrecision(18, 3);
            entity.Property(order => order.UnitPrice).HasPrecision(18, 2);
            entity.Property(order => order.Fee).HasPrecision(18, 2);
            entity.Property(order => order.TotalAmount).HasPrecision(18, 2);
            entity.Property(order => order.TagsJson).IsRequired();
            entity.Property(order => order.Version).IsConcurrencyToken();
            entity.HasIndex(order => new { order.UserId, order.Status, order.CreatedAt });
        });
        modelBuilder.Entity<WalletDocument>(entity =>
        {
            entity.ToTable("gold_wallets");
            entity.HasKey(wallet => wallet.UserId);
            entity.Property(wallet => wallet.BalanceGrams).HasPrecision(18, 3);
            entity.Property(wallet => wallet.Version).IsConcurrencyToken();
        });
    }
}
