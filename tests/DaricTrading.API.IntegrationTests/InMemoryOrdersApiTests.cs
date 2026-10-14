using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using DaricTrading.API.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DaricTrading.API.IntegrationTests;

/// <summary>پاسخ endpointها، validation، health و Swagger را با ذخیره‌سازی حافظه‌ای بررسی می‌کند.</summary>
public sealed class InMemoryOrdersApiTests
{
    /// <summary>ثبت خرید و ویرایش برچسب از مسیرهای نسخه‌بندی‌شده API را بررسی می‌کند.</summary>
    [Fact]
    public async Task BuyAndMetadataUpdate_AreExposedThroughVersionedHttpContracts()
    {
        using var factory = new InMemoryApiFactory();
        using var client = factory.CreateClient();
        var userId = Guid.NewGuid();
        var create = await client.PostAsJsonAsync("/api/v1/orders/buy", new CreateBuyOrderRequest(userId, 1.250m, 3_500_000m, 5_000m, ["پس‌انداز_بلندمدت"]));
        create.StatusCode.Should().Be(HttpStatusCode.Created, await create.Content.ReadAsStringAsync());
        var order = (await create.Content.ReadFromJsonAsync<OrderResponse>())!;

        var update = await client.PatchAsJsonAsync($"/api/v1/orders/{order.Id}/tags", new UpdateOrderTagsRequest(["صندوق_طلای_فیزیکی"], "annual review"));
        update.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updated = await client.GetFromJsonAsync<OrderResponse>($"/api/v1/orders/{order.Id}");
        updated!.TotalAmount.Should().Be(order.TotalAmount);
        updated.UserNote.Should().Be("annual review");

        var wallet = await client.GetFromJsonAsync<WalletBalanceResponse>($"/api/v1/wallets/{userId}/balance");
        wallet!.BalanceGrams.Should().Be(1.250m);
    }

    /// <summary>بازگشت خطاهای اعتبارسنجی در قالب RFC 7807 را بررسی می‌کند.</summary>
    [Fact]
    public async Task InvalidInput_ReturnsRfc7807FieldErrors()
    {
        using var factory = new InMemoryApiFactory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/orders/buy", new CreateBuyOrderRequest(Guid.NewGuid(), 0m, 3_500_000m, 0m, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        var problem = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonDocument>();
        problem!.RootElement.GetProperty("errors").GetProperty("WeightGrams").GetArrayLength().Should().BeGreaterThan(0);
    }

    /// <summary>بازگشت پاسخ ۴۲۲ هنگام فروش بیش از موجودی را بررسی می‌کند.</summary>
    [Fact]
    public async Task SellWithoutBalance_Returns422ProblemDetails()
    {
        using var factory = new InMemoryApiFactory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/orders/sell", new CreateSellOrderRequest(Guid.NewGuid(), 1m, 1_000m, 0m, null));
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>بازگشت پاسخ ۴۰۴ برای کیف پول پیدا نشده را بررسی می‌کند.</summary>
    [Fact]
    public async Task MissingWallet_ReturnsNotFoundProblemDetails()
    {
        using var factory = new InMemoryApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/wallets/{Guid.NewGuid()}/balance");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        var missingOrder = await client.GetAsync($"/api/v1/orders/{Guid.NewGuid()}");
        missingOrder.StatusCode.Should().Be(HttpStatusCode.NotFound);
        missingOrder.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    /// <summary>در دسترس بودن مسیر سلامت و مستندات Swagger را بررسی می‌کند.</summary>
    [Fact]
    public async Task HealthAndSwaggerEndpoints_AreAvailable()
    {
        using var factory = new InMemoryApiFactory();
        using var client = factory.CreateClient();

        var health = await client.GetAsync("/health");
        var swagger = await client.GetAsync("/swagger/v1/swagger.json");

        health.StatusCode.Should().Be(HttpStatusCode.OK);
        swagger.StatusCode.Should().Be(HttpStatusCode.OK);
        (await swagger.Content.ReadAsStringAsync()).Should().Contain("weightGrams");
    }
    /// <summary>پیکربندی میزبان تست را طوری تغییر می‌دهد که repository حافظه‌ای جای PostgreSQL را بگیرد.</summary>
    private sealed class InMemoryApiFactory : WebApplicationFactory<Program>
    {
        /// <summary>میزبان وب آزمون را با خدمات و تنظیمات لازم پیکربندی می‌کند.</summary>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Persistence:Provider"] = "InMemory"
            }));
        }
    }
}

