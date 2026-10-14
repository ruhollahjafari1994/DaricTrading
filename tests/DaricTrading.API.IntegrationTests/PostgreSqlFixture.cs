using Testcontainers.PostgreSql;

namespace DaricTrading.API.IntegrationTests;

/// <summary>کانتینر PostgreSQL را برای کل مجموعه آزمون بالا می‌آورد و پس از آن آزاد می‌کند.</summary>
public sealed class PostgreSqlFixture : IAsyncLifetime
{
    /// <summary>کانتینر PostgreSQL مورد استفاده آزمون‌های یکپارچگی را نگه می‌دارد.</summary>
    public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("daric_test")
        .WithUsername("daric")
        .WithPassword("daric_test_password")
        .Build();

    /// <summary>کانتینر PostgreSQL را پیش از اجرای مجموعه آزمون‌ها راه‌اندازی می‌کند.</summary>
    public Task InitializeAsync() => Container.StartAsync();
    /// <summary>منابع کانتینر PostgreSQL را پس از اتمام آزمون‌ها آزاد می‌کند.</summary>
    public Task DisposeAsync() => Container.DisposeAsync().AsTask();
}

/// <summary>آزمون‌های این مجموعه را به نمونه مشترک PostgreSQL متصل می‌کند.</summary>
[CollectionDefinition(Name)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
    /// <summary>نامی ثابت برای اتصال کلاس‌های آزمون به collection مشترک PostgreSQL.</summary>
    public const string Name = "postgres integration collection";
}
