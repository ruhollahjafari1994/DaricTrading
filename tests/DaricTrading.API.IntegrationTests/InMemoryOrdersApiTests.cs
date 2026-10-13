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

public sealed class InMemoryOrdersApiTests
{
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

    [Fact]
    public async Task SellWithoutBalance_Returns422ProblemDetails()
    {
        using var factory = new InMemoryApiFactory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/orders/sell", new CreateSellOrderRequest(Guid.NewGuid(), 1m, 1_000m, 0m, null));
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

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
    private sealed class InMemoryApiFactory : WebApplicationFactory<Program>
    {
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

