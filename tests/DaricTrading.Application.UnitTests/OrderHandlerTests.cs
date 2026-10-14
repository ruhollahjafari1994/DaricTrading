using DaricTrading.Application.Common;
using DaricTrading.Application.Orders;
using DaricTrading.Application.Orders.Commands;
using DaricTrading.Application.Validation;
using DaricTrading.Domain;
using FluentAssertions;
using Moq;

namespace DaricTrading.Application.UnitTests;

/// <summary>تعامل handlerهای خرید و فروش با repository، کیف پول، واحد کار و event dispatcher را بررسی می‌کند.</summary>
public sealed class OrderHandlerTests
{
    /// <summary>افزایش موجودی، ذخیره سفارش و انتشار رویداد خرید را بررسی می‌کند.</summary>
    [Fact]
    public async Task BuyHandler_CreditsWalletPersistsOrderAndDispatchesSettlementEvent()
    {
        var userId = Guid.NewGuid();
        var wallet = new GoldWallet(userId);
        var orderRepository = new Mock<IGoldOrderRepository>();
        var walletRepository = new Mock<IGoldWalletRepository>();
        var eventDispatcher = new Mock<IDomainEventDispatcher>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var commitSequence = new MockSequence();
        walletRepository.Setup(repository => repository.GetOrCreateAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        walletRepository.Setup(repository => repository.SaveAsync(wallet, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        orderRepository.Setup(repository => repository.AddAsync(It.IsAny<GoldOrder>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        unitOfWork.InSequence(commitSequence).Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        eventDispatcher.InSequence(commitSequence).Setup(dispatcher => dispatcher.DispatchAsync(It.IsAny<IReadOnlyCollection<IDomainEvent>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var handler = new BuyOrderHandler(orderRepository.Object, walletRepository.Object, unitOfWork.Object, eventDispatcher.Object, new FixedClock(), new CreateBuyOrderCommandValidator());

        var result = await handler.HandleAsync(new CreateBuyOrderCommand(userId, 1.250m, 3_500_000m, 5_000m, ["پس‌انداز_بلندمدت"]));

        result.TotalAmount.Should().Be(4_380_000m);
        wallet.Balance.Grams.Should().Be(1.250m);
        orderRepository.Verify(repository => repository.AddAsync(It.Is<GoldOrder>(order => order.UserId == userId && order.Status == OrderStatus.Settled), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        eventDispatcher.Verify(dispatcher => dispatcher.DispatchAsync(It.Is<IReadOnlyCollection<IDomainEvent>>(events => events.Single() is GoldPurchasedDomainEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>رد فروش بدون موجودی کافی و ذخیره‌نشدن سفارش ناموفق را بررسی می‌کند.</summary>
    [Fact]
    public async Task SellHandler_InsufficientBalanceRaisesApplicationBusinessExceptionAndDoesNotPersistOrder()
    {
        var userId = Guid.NewGuid();
        var wallet = new GoldWallet(userId);
        var orderRepository = new Mock<IGoldOrderRepository>();
        var walletRepository = new Mock<IGoldWalletRepository>();
        var eventDispatcher = new Mock<IDomainEventDispatcher>();
        var unitOfWork = new Mock<IUnitOfWork>();
        walletRepository.Setup(repository => repository.GetOrCreateAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        unitOfWork.Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var handler = new SellOrderHandler(orderRepository.Object, walletRepository.Object, unitOfWork.Object, eventDispatcher.Object, new FixedClock(), new CreateSellOrderCommandValidator());

        var act = () => handler.HandleAsync(new CreateSellOrderCommand(userId, 0.250m, 2_000_000m, 1_000m, null));

        await act.Should().ThrowAsync<BusinessRuleViolationException>();
        orderRepository.Verify(repository => repository.AddAsync(It.IsAny<GoldOrder>(), It.IsAny<CancellationToken>()), Times.Never);
        wallet.Balance.Grams.Should().Be(0m);
    }

    /// <summary>ساعت ثابت موردنیاز آزمون‌های handler را پیاده‌سازی می‌کند.</summary>
    private sealed class FixedClock : IClock
    {
        /// <summary>زمان ثابت آزمون را از طریق قرارداد ساعت دامنه برمی‌گرداند.</summary>
        public DateTimeOffset UtcNow => new(2026, 10, 13, 0, 0, 0, TimeSpan.Zero);
    }
}
