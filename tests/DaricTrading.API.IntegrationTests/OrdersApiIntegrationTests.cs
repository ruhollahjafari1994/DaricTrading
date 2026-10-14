using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using DaricTrading.API.Contracts;
using DaricTrading.Application.Common;
using DaricTrading.Domain;
using DaricTrading.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DaricTrading.API.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
/// <summary>ثبت و جست‌وجوی سفارش، تراکنش PostgreSQL و سناریوهای رقابت را از مسیر HTTP بررسی می‌کند.</summary>
public sealed class OrdersApiIntegrationTests : IAsyncLifetime
{
    /// <summary>fixtureای را نگه می‌دارد که connection string کانتینر PostgreSQL را فراهم می‌کند.</summary>
    private readonly PostgreSqlFixture _database;
    /// <summary>میزبان تست ASP.NET را می‌سازد و سرویس‌های API را به پایگاه داده آزمون وصل می‌کند.</summary>
    private DaricApiFactory _factory = null!;
    /// <summary>کلاینت HTTP برای فراخوانی endpointها در آزمون‌های یکپارچگی.</summary>
    private HttpClient _client = null!;

    /// <summary>رفتار API سفارش و سازگاری ذخیره‌سازی PostgreSQL را آزمون می‌کند.</summary>
    public OrdersApiIntegrationTests(PostgreSqlFixture database) => _database = database;

    /// <summary>کانتینر PostgreSQL را پیش از اجرای مجموعه آزمون‌ها راه‌اندازی می‌کند.</summary>
    public Task InitializeAsync()
    {
        _factory = new DaricApiFactory(_database.Container.GetConnectionString());
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    /// <summary>منابع کانتینر PostgreSQL را پس از اتمام آزمون‌ها آزاد می‌کند.</summary>
    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    /// <summary>ثبت سفارش خرید و ماندگاری موجودی در PostgreSQL را بررسی می‌کند.</summary>
    [Fact]
    public async Task BuyOrder_IsCreatedAndBalanceIsPersisted()
    {
        var userId = Guid.NewGuid();
        var response = await _client.PostAsJsonAsync("/api/v1/orders/buy", new CreateBuyOrderRequest(userId, 1.250m, 3_500_000m, 5_000m, ["پس‌انداز_بلندمدت"]));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order.Should().NotBeNull();
        order.TotalAmount.Should().Be(4_380_000m);

        var balance = await _client.GetFromJsonAsync<WalletBalanceResponse>($"/api/v1/wallets/{userId}/balance");
        balance.Should().NotBeNull();
        balance!.BalanceGrams.Should().Be(1.250m);
    }

    /// <summary>رد فروش بیش از موجودی و پاسخ ۴۲۲ را بررسی می‌کند.</summary>
    [Fact]
    public async Task SellWithoutBalance_ReturnsUnprocessableEntity()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/orders/sell", new CreateSellOrderRequest(Guid.NewGuid(), 0.001m, 3_500_000m, 100m, null));
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>ویرایش برچسب و جست‌وجو را بدون تغییر مقادیر مالی بررسی می‌کند.</summary>
    [Fact]
    public async Task TagUpdateAndSearch_PreserveSettledFinancialValues()
    {
        var userId = Guid.NewGuid();
        var createdResponse = await _client.PostAsJsonAsync("/api/v1/orders/buy", new CreateBuyOrderRequest(userId, 0.500m, 2_000_000m, 1_000m, ["پس‌انداز_بلندمدت"]));
        var created = await createdResponse.Content.ReadFromJsonAsync<OrderResponse>();
        created.Should().NotBeNull();
        var originalTotal = created!.TotalAmount;

        var updatedResponse = await _client.PatchAsJsonAsync($"/api/v1/orders/{created!.Id}/tags", new UpdateOrderTagsRequest(["صندوق_طلای_فیزیکی"], "tax lot 2026"));
        updatedResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var updated = await _client.GetFromJsonAsync<OrderResponse>($"/api/v1/orders/{created!.Id}");
        updated.Should().NotBeNull();
        updated!.TotalAmount.Should().Be(originalTotal);
        updated.Tags.Should().ContainSingle().Which.Should().Be("صندوق_طلای_فیزیکی");
        updated.UserNote.Should().Be("tax lot 2026");

        var from = Uri.EscapeDataString(DateTimeOffset.UtcNow.AddMinutes(-2).ToString("O"));
        var to = Uri.EscapeDataString(DateTimeOffset.UtcNow.AddMinutes(2).ToString("O"));
        var search = await _client.GetFromJsonAsync<OrderResponse[]>($"/api/v1/orders?userId={userId}&tag={Uri.EscapeDataString("  صندوق_طلای_فیزیکی  ")}&from={from}&to={to}&orderType=Buy");
        search.Should().ContainSingle().Which.Id.Should().Be(created!.Id);
    }

    /// <summary>امکان لغو سفارش در انتظار را بررسی می‌کند.</summary>
    [Fact]
    public async Task PendingOrder_CanBeCancelled()
    {
        var userId = Guid.NewGuid();
        var pending = GoldOrder.Pending(userId, OrderType.Buy, GoldWeight.FromGrams(0.250m), new Money(1_000_000m), new Money(1_000m));
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<IGoldOrderRepository>().AddAsync(pending);
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
        }

        var response = await _client.PostAsync($"/api/v1/orders/{pending.Id}/cancel", null);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var order = await _client.GetFromJsonAsync<OrderResponse>($"/api/v1/orders/{pending.Id}");
        order!.Status.Should().Be("Cancelled");
    }

    /// <summary>جلوگیری از منفی شدن موجودی ذخیره‌شده در فروش‌های هم‌زمان را بررسی می‌کند.</summary>
    [Fact]
    public async Task ConcurrentSales_NeverDrivePersistedWalletBelowZero()
    {
        var userId = Guid.NewGuid();
        await _client.PostAsJsonAsync("/api/v1/orders/buy", new CreateBuyOrderRequest(userId, 1m, 2_000_000m, 0m, null));
        var requests = Enumerable.Range(0, 20).Select(_ => _client.PostAsJsonAsync("/api/v1/orders/sell", new CreateSellOrderRequest(userId, 0.250m, 2_000_000m, 0m, null)));
        var responses = await Task.WhenAll(requests);
        var statuses = responses.Select(response => response.StatusCode).ToArray();
        statuses.Should().OnlyContain(status => status == HttpStatusCode.Created || status == HttpStatusCode.Conflict || status == HttpStatusCode.UnprocessableEntity);
        var successfulSales = statuses.Count(status => status == HttpStatusCode.Created);
        successfulSales.Should().BeInRange(1, 4);

        var wallet = await _client.GetFromJsonAsync<WalletBalanceResponse>($"/api/v1/wallets/{userId}/balance");
        wallet!.BalanceGrams.Should().BeGreaterThanOrEqualTo(0m);
        wallet.BalanceGrams.Should().Be(1m - successfulSales * 0.250m);
    }

    /// <summary>حفظ اعتبار موجودی در خریدهای هم‌زمان اولیه را بررسی می‌کند.</summary>
    [Fact]
    public async Task ConcurrentFirstBuys_DoNotLoseWalletCredits()
    {
        var userId = Guid.NewGuid();
        var requests = Enumerable.Range(0, 12)
            .Select(_ => _client.PostAsJsonAsync(
                "/api/v1/orders/buy",
                new CreateBuyOrderRequest(userId, 0.125m, 2_000_000m, 0m, null)));

        var responses = await Task.WhenAll(requests);
        var statuses = responses.Select(response => response.StatusCode).ToArray();
        statuses.Should().OnlyContain(status =>
            status == HttpStatusCode.Created || status == HttpStatusCode.Conflict);

        var successfulBuys = statuses.Count(status => status == HttpStatusCode.Created);
        successfulBuys.Should().BeInRange(1, 12);

        var wallet = await _client.GetFromJsonAsync<WalletBalanceResponse>(
            $"/api/v1/wallets/{userId}/balance");
        wallet!.BalanceGrams.Should().Be(successfulBuys * 0.125m);
    }

    /// <summary>پیکربندی میزبان تست را به connection string کانتینر PostgreSQL وصل می‌کند.</summary>
    private sealed class DaricApiFactory(string connectionString) : WebApplicationFactory<Program>
    {
        /// <summary>میزبان وب آزمون را با خدمات و تنظیمات لازم پیکربندی می‌کند.</summary>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Persistence:Provider"] = "PostgreSql",
                ["ConnectionStrings:Trading"] = connectionString
            }));
        }
    }
}

