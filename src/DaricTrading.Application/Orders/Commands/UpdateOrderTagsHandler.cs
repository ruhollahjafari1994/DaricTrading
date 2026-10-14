using DaricTrading.Application.Common;
using DaricTrading.Application.Orders;
using DaricTrading.Domain;
using FluentValidation;

namespace DaricTrading.Application.Orders.Commands;

/// <summary>موردکار ویرایش برچسب و یادداشت سفارش را بدون دست‌کاری مقادیر مالی اجرا می‌کند.</summary>
public sealed class UpdateOrderTagsHandler(IGoldOrderRepository orders, IUnitOfWork unitOfWork, IValidator<UpdateOrderTagsCommand> validator)
{
    /// <summary>سفارش را بازیابی، تگ و یادداشت را validate و update کرده و فقط metadata را ذخیره می‌کند.</summary>
    public Task<OrderDto> HandleAsync(UpdateOrderTagsCommand command, CancellationToken cancellationToken = default) => DomainExceptionBoundary.ExecuteAsync(async () =>
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var order = await orders.GetAsync(command.OrderId, cancellationToken) ?? throw new NotFoundException("Order", command.OrderId);
        order.UpdateTags(command.Tags.Select(value => new Tag(value)), command.Note);
        await orders.UpdateAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OrderDto.From(order);
    });
}
