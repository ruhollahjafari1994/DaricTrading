using DaricTrading.Application.Common;
using DaricTrading.Application.Orders;
using DaricTrading.Domain;
using FluentValidation;

namespace DaricTrading.Application.Orders.Commands;

/// <summary>موردکار لغو سفارش را اجرا می‌کند و تغییر وضعیت و رویداد لغو را ذخیره و منتشر می‌کند.</summary>
public sealed class CancelOrderHandler(IGoldOrderRepository orders, IUnitOfWork unitOfWork, IDomainEventDispatcher eventDispatcher, IClock clock, IValidator<CancelOrderCommand> validator)
{
    /// <summary>سفارش را پیدا می‌کند، لغو مجاز را اعمال و تغییر را ذخیره می‌کند؛ سپس event لغو را منتشر می‌کند.</summary>
    public Task<OrderDto> HandleAsync(CancelOrderCommand command, CancellationToken cancellationToken = default) => DomainExceptionBoundary.ExecuteAsync(async () =>
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var order = await orders.GetAsync(command.OrderId, cancellationToken) ?? throw new NotFoundException("Order", command.OrderId);
        order.Cancel(clock.UtcNow);
        await orders.UpdateAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await eventDispatcher.DispatchAsync(order.DequeueDomainEvents(), cancellationToken);
        return OrderDto.From(order);
    });
}
