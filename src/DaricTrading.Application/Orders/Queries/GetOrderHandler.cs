using DaricTrading.Application.Common;
using DaricTrading.Application.Orders;
using DaricTrading.Domain;
using FluentValidation;

namespace DaricTrading.Application.Orders.Queries;

/// <summary>موردکار خواندن یک سفارش را بر اساس شناسه اجرا می‌کند.</summary>
public sealed class GetOrderHandler(IGoldOrderRepository orders, IValidator<GetOrderQuery> validator)
{
    /// <summary>شناسه را validate، سفارش را از repository دریافت و به OrderDto تبدیل می‌کند.</summary>
    public async Task<OrderDto> HandleAsync(GetOrderQuery query, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);
        var order = await orders.GetAsync(query.OrderId, cancellationToken)
            ?? throw new NotFoundException("Order", query.OrderId);

        return OrderDto.From(order);
    }
}
