using DaricTrading.Domain;
using FluentAssertions;

namespace DaricTrading.Domain.UnitTests;

public sealed class TradingDomainTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-0.001)]
    [InlineData(1.0001)]
    public void GoldWeight_RejectsNonPositiveOrExcessPrecision(decimal grams) =>
        FluentActions.Invoking(() => GoldWeight.FromGrams(grams)).Should().Throw<DomainRuleViolationException>();

    [Fact]
    public void GoldWeight_AcceptsMilligramPrecisionAndSupportsArithmetic()
    {
        var first = GoldWeight.FromGrams(12.450m);
        var second = GoldWeight.FromGrams(0.050m);
        (first + second).Grams.Should().Be(12.500m);
        first.CompareTo(second).Should().BePositive();
    }

    [Fact]
    public void Wallet_DebitCannotOversellAndLeavesBalanceUnchanged()
    {
        var wallet = new GoldWallet(Guid.NewGuid());
        wallet.Credit(GoldWeight.FromGrams(1m));
        FluentActions.Invoking(() => wallet.Debit(GoldWeight.FromGrams(1.001m))).Should().Throw<InsufficientGoldBalanceException>();
        wallet.Balance.Grams.Should().Be(1m);
    }

    [Fact]
    public void ConcurrentDebits_CannotOversellWallet()
    {
        var wallet = new GoldWallet(Guid.NewGuid());
        wallet.Credit(GoldWeight.FromGrams(10m));
        var successes = 0;
        Parallel.For(0, 20, _ =>
        {
            try { wallet.Debit(GoldWeight.FromGrams(1m)); Interlocked.Increment(ref successes); }
            catch (InsufficientGoldBalanceException) { }
        });
        successes.Should().Be(10);
        wallet.Balance.Grams.Should().Be(0m);
    }

    [Fact]
    public void BuyAndSellOrders_ApplyTheCorrectFeeFormulaAndRaiseEvents()
    {
        var userId = Guid.NewGuid();
        var weight = GoldWeight.FromGrams(1.25m);
        var price = new Money(1_000m);
        var fee = new Money(25m);
        var buy = GoldOrder.Buy(userId, weight, price, fee);
        var sell = GoldOrder.Sell(userId, weight, price, fee);
        buy.TotalAmount.Amount.Should().Be(1_275m);
        sell.TotalAmount.Amount.Should().Be(1_225m);
        buy.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<GoldPurchasedDomainEvent>();
        sell.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<GoldSoldDomainEvent>();
    }

    [Fact]
    public void SettledOrder_AllowsMetadataChangesButPreservesFinancialAttributes()
    {
        var order = GoldOrder.Buy(Guid.NewGuid(), GoldWeight.FromGrams(1m), new Money(100m), new Money(5m));
        var total = order.TotalAmount;
        order.UpdateTags([new Tag("پس‌انداز_بلندمدت")], "long-term holding");
        order.TotalAmount.Should().Be(total);
        order.UserNote.Should().Be("long-term holding");
        order.Tags.Should().ContainSingle();
    }

    [Fact]
    public void OnlyPendingOrderCanBeCancelled_AndRaisesEvent()
    {
        var pending = GoldOrder.Pending(Guid.NewGuid(), OrderType.Buy, GoldWeight.FromGrams(1m), new Money(100m), new Money(2m));
        pending.Cancel();
        pending.Status.Should().Be(OrderStatus.Cancelled);
        pending.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderCancelledDomainEvent>();

        var settled = GoldOrder.Buy(Guid.NewGuid(), GoldWeight.FromGrams(1m), new Money(100m), new Money(2m));
        FluentActions.Invoking(() => settled.Cancel()).Should().Throw<InvalidOrderStateTransitionException>();
    }
}
