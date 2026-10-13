using System.Text.Json;
using DaricTrading.Domain;
using Microsoft.EntityFrameworkCore;

namespace DaricTrading.Infrastructure.Persistence.PostgreSql;

public sealed class TradingDbContext(DbContextOptions<TradingDbContext> options) : DbContext(options)
{
    public DbSet<OrderDocument> Orders => Set<OrderDocument>();
    public DbSet<WalletDocument> Wallets => Set<WalletDocument>();

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

public sealed class OrderDocument
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Type { get; set; }
    public decimal WeightGrams { get; set; }
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "IRR";
    public decimal Fee { get; set; }
    public decimal TotalAmount { get; set; }
    public string TagsJson { get; set; } = "[]";
    public int Status { get; set; }
    public string? UserNote { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? SettledAt { get; set; }
    public long Version { get; set; }

    public static OrderDocument From(GoldOrder order, long version = 0) => new()
    {
        Id = order.Id,
        UserId = order.UserId,
        Type = (int)order.Type,
        WeightGrams = order.Weight.Grams,
        UnitPrice = order.UnitPrice.Amount,
        Currency = order.UnitPrice.Currency,
        Fee = order.Fee.Amount,
        TotalAmount = order.TotalAmount.Amount,
        TagsJson = JsonSerializer.Serialize(order.Tags.Select(tag => tag.Value)),
        Status = (int)order.Status,
        UserNote = order.UserNote,
        CreatedAt = order.CreatedAt,
        SettledAt = order.SettledAt,
        Version = version
    };
    public GoldOrder ToDomain() => GoldOrder.Restore(Id, UserId, (OrderType)Type, GoldWeight.FromGrams(WeightGrams), new Money(UnitPrice, Currency), new Money(Fee, Currency), JsonSerializer.Deserialize<string[]>(TagsJson)!.Select(value => new Tag(value)), (OrderStatus)Status, UserNote, CreatedAt, SettledAt);
}

public sealed class WalletDocument
{
    public Guid UserId { get; set; }
    public decimal BalanceGrams { get; set; }
    public long Version { get; set; }
}
