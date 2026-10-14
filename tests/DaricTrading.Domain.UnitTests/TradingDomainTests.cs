using DaricTrading.Domain;
using FluentAssertions;

namespace DaricTrading.Domain.UnitTests;

/// <summary>قواعد ارزش‌اشیا، موجودی کیف پول، قیمت‌گذاری و وضعیت‌های سفارش را بدون زیرساخت بررسی می‌کند.</summary>
public sealed class TradingDomainTests
{
    /// <summary>وزن صفر، منفی یا دارای بیش از سه رقم اعشار را نامعتبر می‌داند.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-0.001)]
    [InlineData(0.0001)]
    [InlineData(1.0001)]
    public void GoldWeight_RejectsNonPositiveOrExcessPrecision(decimal grams) =>
        FluentActions.Invoking(() => GoldWeight.FromGrams(grams)).Should().Throw<DomainRuleViolationException>();

    /// <summary>پذیرش دقت یک میلی‌گرم و انجام محاسبات وزن را بررسی می‌کند.</summary>
    [Fact]
    public void GoldWeight_AcceptsMilligramPrecisionAndSupportsArithmetic()
    {
        GoldWeight.FromGrams(0.001m).Grams.Should().Be(0.001m);

        var first = GoldWeight.FromGrams(12.450m);
        var second = GoldWeight.FromGrams(0.050m);
        (first + second).Grams.Should().Be(12.500m);
        first.CompareTo(second).Should().BePositive();
    }

    /// <summary>جلوگیری از ایجاد وزن صفر یا منفی هنگام تفریق را بررسی می‌کند.</summary>
    [Fact]
    public void GoldWeight_SubtractionCannotProduceZeroOrNegativeWeight()
    {
        var weight = GoldWeight.FromGrams(0.001m);

        FluentActions.Invoking(() => weight - GoldWeight.FromGrams(0.001m))
            .Should().Throw<DomainRuleViolationException>();
        FluentActions.Invoking(() => weight - GoldWeight.FromGrams(0.002m))
            .Should().Throw<DomainRuleViolationException>();
    }

    /// <summary>پاک‌سازی فاصله‌های برچسب و پذیرش مقادیر مجاز را بررسی می‌کند.</summary>
    [Fact]
    public void Tag_TrimsOuterWhitespaceAndKeepsOnlyCanonicalAllowedValues()
    {
        var tag = new Tag("  پس‌انداز_بلندمدت  ");

        tag.Value.Should().Be("پس‌انداز_بلندمدت");
    }

    /// <summary>درستی محاسبات مبلغ، حفظ واحد پول و غیرمنفی ماندن نتیجه را بررسی می‌کند.</summary>
    [Fact]
    public void Money_ArithmeticPreservesCurrencyAndNonNegativeInvariant()
    {
        var amount = new Money(125.25m, "IRR");
        new Money(1m, "USD").Currency.Should().Be("USD");

        (amount + new Money(10m, "IRR")).Amount.Should().Be(135.25m);
        FluentActions.Invoking(() => amount - new Money(126m, "IRR"))
            .Should().Throw<DomainRuleViolationException>();
        FluentActions.Invoking(() => amount + new Money(1m, "USD"))
            .Should().Throw<DomainRuleViolationException>();
    }

    /// <summary>رد مبلغ‌هایی را که از دقت ذخیره‌سازی پشتیبانی‌شده بیشترند بررسی می‌کند.</summary>
    [Fact]
    public void Money_RejectsValuesOutsideTheStoragePrecisionRange()
    {
        new Money(Money.MaximumSupportedAmount).Amount.Should().Be(Money.MaximumSupportedAmount);

        FluentActions.Invoking(() => new Money(Money.MaximumSupportedAmount + 0.01m))
            .Should().Throw<DomainRuleViolationException>();
    }

    /// <summary>رد کدهای واحد پول پشتیبانی‌نشده را بررسی می‌کند.</summary>
    [Fact]
    public void Money_RejectsCurrenciesThatAreNotSupported()
    {
        FluentActions.Invoking(() => new Money(1m, "ZZZ"))
            .Should().Throw<DomainRuleViolationException>();
        FluentActions.Invoking(() => new Money(1m, "usd"))
            .Should().Throw<DomainRuleViolationException>();
    }

    /// <summary>اعتبارسنجی شناسه و سازگاری وضعیت تسویه هنگام بازسازی سفارش را بررسی می‌کند.</summary>
    [Fact]
    public void RestoringOrder_RejectsInvalidIdentityAndSettlementState()
    {
        var userId = Guid.NewGuid();
        var weight = GoldWeight.FromGrams(1m);
        var price = new Money(100m);
        var fee = new Money(1m);

        FluentActions.Invoking(() => GoldOrder.Restore(
                Guid.Empty, userId, OrderType.Buy, weight, price, fee, [], OrderStatus.Settled,
                null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow))
            .Should().Throw<DomainRuleViolationException>();
        FluentActions.Invoking(() => GoldOrder.Restore(
                Guid.NewGuid(), userId, OrderType.Buy, weight, price, fee, [], OrderStatus.Settled,
                null, DateTimeOffset.UtcNow, null))
            .Should().Throw<DomainRuleViolationException>();
    }

    /// <summary>جلوگیری از فروش بیش از موجودی و ثابت ماندن موجودی پس از خطا را بررسی می‌کند.</summary>
    [Fact]
    public void Wallet_DebitCannotOversellAndLeavesBalanceUnchanged()
    {
        var wallet = new GoldWallet(Guid.NewGuid());
        wallet.Credit(GoldWeight.FromGrams(1m));
        FluentActions.Invoking(() => wallet.Debit(GoldWeight.FromGrams(1.001m))).Should().Throw<InsufficientGoldBalanceException>();
        wallet.Balance.Grams.Should().Be(1m);
    }

    /// <summary>جلوگیری از فروش بیش از موجودی در برداشت‌های هم‌زمان را بررسی می‌کند.</summary>
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

    /// <summary>فرمول مبلغ خرید و فروش و ثبت رویدادهای متناظر را بررسی می‌کند.</summary>
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
        buy.DequeueDomainEvents().Should().ContainSingle();
        buy.DomainEvents.Should().BeEmpty();
    }

    /// <summary>ویرایش اطلاعات توصیفی سفارش تسویه‌شده و تغییرنکردن فیلدهای مالی را بررسی می‌کند.</summary>
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

    /// <summary>محدود بودن لغو به سفارش در انتظار و ثبت رویداد لغو را بررسی می‌کند.</summary>
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
