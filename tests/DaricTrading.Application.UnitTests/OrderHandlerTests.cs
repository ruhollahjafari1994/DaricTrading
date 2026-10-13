using DaricTrading.Application.Common;
using DaricTrading.Application.Orders;
using DaricTrading.Application.Validation;
using DaricTrading.Domain;
using FluentAssertions;
using Moq;

namespace DaricTrading.Application.UnitTests;

public sealed class OrderHandlerTests
{
    [Fact]
    public async Task BuyHandler_CreditsWalletPersistsOrderAndDispatchesSettlementEvent()
    {
        var userId = Guid.NewGuid();
        var wallet = new GoldWallet(userId);
        var orderRepository = new Mock<IGoldOrderRepository>();
        var walletRepository = new Mock<IGoldWalletRepository>();
        var eventDispatcher = new Mock<IDomainEventDispatcher>();
        walletRepository.Setup(repository => repository.GetOrCreateAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        walletRepository.Setup(repository => repository.SaveAsync(wallet, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        orderRepository.Setup(repository => repository.AddAsync(It.IsAny<GoldOrder>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        eventDispatcher.Setup(dispatcher => dispatcher.DispatchAsync(It.IsAny<IReadOnlyCollection<IDomainEvent>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var handler = new BuyOrderHandler(orderRepository.Object, walletRepository.Object, eventDispatcher.Object, new FixedClock(), new CreateOrderCommandValidator());

        var result = await handler.HandleAsync(new CreateOrderCommand(userId, 1.250m, 3_500_000m, 5_000m, ["پس‌انداز_بلندمدت"]));

        result.TotalAmount.Should().Be(4_380_000m);
        wallet.Balance.Grams.Should().Be(1.250m);
        orderRepository.Verify(repository => repository.AddAsync(It.Is<GoldOrder>(order => order.UserId == userId && order.Status == OrderStatus.Settled), It.IsAny<CancellationToken>()), Times.Once);
        eventDispatcher.Verify(dispatcher => dispatcher.DispatchAsync(It.Is<IReadOnlyCollection<IDomainEvent>>(events => events.Single() is GoldPurchasedDomainEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SellHandler_InsufficientBalanceRaisesApplicationBusinessExceptionAndDoesNotPersistOrder()
    {
        var userId = Guid.NewGuid();
        var wallet = new GoldWallet(userId);
        var orderRepository = new Mock<IGoldOrderRepository>();
        var walletRepository = new Mock<IGoldWalletRepository>();
        var eventDispatcher = new Mock<IDomainEventDispatcher>();
        walletRepository.Setup(repository => repository.GetOrCreateAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        var handler = new SellOrderHandler(orderRepository.Object, walletRepository.Object, eventDispatcher.Object, new FixedClock(), new CreateOrderCommandValidator());

        var act = () => handler.HandleAsync(new CreateOrderCommand(userId, 0.250m, 2_000_000m, 1_000m, null));

        await act.Should().ThrowAsync<BusinessRuleViolationException>();
        orderRepository.Verify(repository => repository.AddAsync(It.IsAny<GoldOrder>(), It.IsAny<CancellationToken>()), Times.Never);
        wallet.Balance.Grams.Should().Be(0m);
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 10, 13, 0, 0, 0, TimeSpan.Zero);
    }
}
