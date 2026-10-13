using DaricTrading.Domain;

namespace DaricTrading.Domain.UnitTests;

public sealed class TradingDomainTests
{
    [Fact]
    public void GoldWeight_RequiresPositiveValueWithMaximumThreeDecimals()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GoldWeight.FromGrams(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => GoldWeight.FromGrams(1.0001m));
        Assert.Equal(1.125m, GoldWeight.FromGrams(1.125m).Grams);
    }

    [Fact]
    public void Wallet_DebitCannotOversell()
    {
        var wallet = new GoldWallet(Guid.NewGuid(), GoldWeight.FromGrams(1m));
        Assert.Throws<InvalidOperationException>(() => wallet.Debit(GoldWeight.FromGrams(1.001m)));
        Assert.Equal(1m, wallet.Balance.Grams);
    }

    [Fact]
    public void PendingOrder_CanBeCancelledButSettledCannot()
    {
        var user = Guid.NewGuid();
        var pending = GoldOrder.Pending(user, OrderType.Buy, GoldWeight.FromGrams(1m), new Money(100m), new Money(1m));
        pending.Cancel();
        Assert.Equal(OrderStatus.Cancelled, pending.Status);
        var settled = GoldOrder.Buy(user, GoldWeight.FromGrams(1m), new Money(100m), new Money(1m));
        Assert.Throws<InvalidOperationException>(() => settled.Cancel());
    }

    [Fact]
    public void BuyOrder_ComputesTotalAndKeepsFinancialFieldsImmutable()
    {
        var order = GoldOrder.Buy(Guid.NewGuid(), GoldWeight.FromGrams(1.25m), new Money(1000m), new Money(25m));
        Assert.Equal(1275m, order.TotalAmount.Amount);
        Assert.Equal(1.25m, order.Weight.Grams);
    }
}
