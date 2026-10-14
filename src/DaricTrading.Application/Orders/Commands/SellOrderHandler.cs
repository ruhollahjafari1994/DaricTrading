using DaricTrading.Application.Common;
using DaricTrading.Application.Orders;
using DaricTrading.Domain;
using FluentValidation;

namespace DaricTrading.Application.Orders.Commands;

/// <summary>موردکار ثبت فروش را اجرا می‌کند و پیش از ذخیره، کفایت موجودی طلا را کنترل می‌کند.</summary>
public sealed class SellOrderHandler(IGoldOrderRepository orders, IGoldWalletRepository wallets, IUnitOfWork unitOfWork, IDomainEventDispatcher eventDispatcher, IClock clock, IValidator<CreateSellOrderCommand> validator)
{
    /// <summary>ورودی را validate می‌کند، موجودی را debit و order فروش را در یک commit ذخیره می‌کند؛ سپس event را می‌فرستد.</summary>
    public Task<OrderDto> HandleAsync(CreateSellOrderCommand command, CancellationToken cancellationToken = default) => DomainExceptionBoundary.ExecuteAsync(async () =>
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var weight = GoldWeight.FromGrams(command.WeightGrams);
        var tags = (command.Tags ?? Array.Empty<string>()).Select(value => new Tag(value)).ToArray();
        var order = GoldOrder.Sell(command.UserId, weight, new Money(command.UnitPrice), new Money(command.Fee), tags, clock.UtcNow);
        var wallet = await wallets.GetOrCreateAsync(command.UserId, cancellationToken);
        wallet.Debit(weight);
        await wallets.SaveAsync(wallet, cancellationToken);
        await orders.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await eventDispatcher.DispatchAsync(order.DequeueDomainEvents(), cancellationToken);
        return OrderDto.From(order);
    });
}
