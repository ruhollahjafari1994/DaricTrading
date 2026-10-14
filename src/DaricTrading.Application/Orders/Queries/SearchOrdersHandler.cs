using DaricTrading.Application.Common;
using DaricTrading.Application.Orders;
using DaricTrading.Domain;
using FluentValidation;

namespace DaricTrading.Application.Orders.Queries;

/// <summary>موردکار جست‌وجو و فیلتر سفارش‌های کاربر را اجرا می‌کند.</summary>
public sealed class SearchOrdersHandler(IGoldOrderRepository orders, IValidator<SearchOrdersQuery> validator)
{
    /// <summary>فیلترها را validate، نوع سفارش را parse و نتیجه repository را به DTO تبدیل می‌کند.</summary>
    public async Task<IReadOnlyList<OrderDto>> HandleAsync(SearchOrdersQuery query, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);
        OrderType? type = string.IsNullOrWhiteSpace(query.Type) ? null : Enum.Parse<OrderType>(query.Type, true);
        var tag = query.Tag is null ? null : new Tag(query.Tag).Value;
        var result = await orders.SearchAsync(query.UserId, tag, query.From, query.To, type, cancellationToken);
        return result.Select(OrderDto.From).ToArray();
    }
}
