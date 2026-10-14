using DaricTrading.Application.Common;
using DaricTrading.Application.Orders;
using DaricTrading.Domain;
using FluentValidation;

namespace DaricTrading.Application.Orders.Commands;

/// <summary>موردکار ثبت خرید را اجرا می‌کند: اعتبارسنجی، افزایش موجودی، ذخیره اتمیک و انتشار رویداد.</summary>
public sealed class BuyOrderHandler(IGoldOrderRepository orders, IGoldWalletRepository wallets, IUnitOfWork unitOfWork, IDomainEventDispatcher eventDispatcher, IClock clock, IValidator<CreateBuyOrderCommand> validator)
{
    /// <summary>ورودی را validate می‌کند، order خرید می‌سازد، موجودی را credit و order را commit می‌کند؛ سپس event را dispatch می‌کند.</summary>
    public Task<OrderDto> HandleAsync(CreateBuyOrderCommand command, CancellationToken cancellationToken = default) => DomainExceptionBoundary.ExecuteAsync(async () =>
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var weight = GoldWeight.FromGrams(command.WeightGrams);
        var tags = (command.Tags ?? Array.Empty<string>()).Select(value => new Tag(value)).ToArray();
        var order = GoldOrder.Buy(command.UserId, weight, new Money(command.UnitPrice), new Money(command.Fee), tags, clock.UtcNow);
        var wallet = await wallets.GetOrCreateAsync(command.UserId, cancellationToken);
        wallet.Credit(weight);
        await wallets.SaveAsync(wallet, cancellationToken);
        await orders.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await eventDispatcher.DispatchAsync(order.DequeueDomainEvents(), cancellationToken);
        return OrderDto.From(order);
    });
}
