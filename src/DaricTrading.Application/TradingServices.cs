using DaricTrading.Domain;
using FluentValidation;

namespace DaricTrading.Application.Orders
{

public sealed class NotFoundException(string resource, Guid id) : Exception($"{resource} '{id}' was not found.");

public sealed record CreateOrderCommand(Guid UserId, decimal WeightGrams, decimal UnitPrice, decimal Fee, IReadOnlyCollection<string>? Tags);
public sealed record UpdateOrderTagsCommand(Guid OrderId, IReadOnlyCollection<string> Tags, string? Note);
public sealed record OrderSearchQuery(Guid UserId, string? Tag = null, DateTimeOffset? From = null, DateTimeOffset? To = null, string? Type = null);
public sealed record OrderDto(Guid Id, Guid UserId, string Type, decimal WeightGrams, decimal UnitPrice, decimal Fee, decimal TotalAmount, string Currency, IReadOnlyCollection<string> Tags, string? Note, string Status, DateTimeOffset CreatedAt)
{
    public static OrderDto From(GoldOrder order) => new(order.Id, order.UserId, order.Type.ToString(), order.Weight.Grams, order.UnitPrice.Amount, order.Fee.Amount, order.TotalAmount.Amount, order.TotalAmount.Currency, order.Tags.Select(tag => tag.Value).ToArray(), order.Note, order.Status.ToString(), order.CreatedAt);
}

public sealed class BuyOrderHandler(IGoldOrderRepository orders, IGoldWalletRepository wallets, IValidator<CreateOrderCommand> validator)
{
    public async Task<OrderDto> HandleAsync(CreateOrderCommand command, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(command, ct);
        var weight = GoldWeight.FromGrams(command.WeightGrams);
        var tags = (command.Tags ?? Array.Empty<string>()).Select(value => new Tag(value)).ToArray();
        var order = GoldOrder.Buy(command.UserId, weight, new Money(command.UnitPrice), new Money(command.Fee), tags);
        var wallet = await wallets.GetOrCreateAsync(command.UserId, ct);
        wallet.Credit(weight);
        await orders.AddAsync(order, ct);
        return OrderDto.From(order);
    }
}

public sealed class SellOrderHandler(IGoldOrderRepository orders, IGoldWalletRepository wallets, IValidator<CreateOrderCommand> validator)
{
    public async Task<OrderDto> HandleAsync(CreateOrderCommand command, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(command, ct);
        var weight = GoldWeight.FromGrams(command.WeightGrams);
        var tags = (command.Tags ?? Array.Empty<string>()).Select(value => new Tag(value)).ToArray();
        var order = GoldOrder.Sell(command.UserId, weight, new Money(command.UnitPrice), new Money(command.Fee), tags);
        var wallet = await wallets.GetOrCreateAsync(command.UserId, ct);
        wallet.Debit(weight);
        await orders.AddAsync(order, ct);
        return OrderDto.From(order);
    }
}

public sealed class GetOrderHandler(IGoldOrderRepository orders)
{
    public async Task<OrderDto?> HandleAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orders.GetAsync(id, ct);
        return order is null ? null : OrderDto.From(order);
    }
}

public sealed class SearchOrdersHandler(IGoldOrderRepository orders, IValidator<OrderSearchQuery> validator)
{
    public async Task<IReadOnlyList<OrderDto>> HandleAsync(OrderSearchQuery query, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(query, ct);
        OrderType? type = string.IsNullOrWhiteSpace(query.Type) ? null : Enum.Parse<OrderType>(query.Type, true);
        var result = await orders.SearchAsync(query.UserId, query.Tag, query.From, query.To, type, ct);
        return result.Select(OrderDto.From).ToArray();
    }
}

public sealed class UpdateOrderTagsHandler(IGoldOrderRepository orders, IValidator<UpdateOrderTagsCommand> validator)
{
    public async Task<OrderDto> HandleAsync(UpdateOrderTagsCommand command, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(command, ct);
        var order = await orders.GetAsync(command.OrderId, ct) ?? throw new NotFoundException("Order", command.OrderId);
        order.UpdateTags(command.Tags.Select(value => new Tag(value)), command.Note);
        return OrderDto.From(order);
    }
}

public sealed class CancelOrderHandler(IGoldOrderRepository orders)
{
    public async Task<OrderDto> HandleAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orders.GetAsync(id, ct) ?? throw new NotFoundException("Order", id);
        order.Cancel();
        return OrderDto.From(order);
    }
}

}
namespace DaricTrading.Application.Wallets
{
    public sealed record WalletBalanceDto(Guid UserId, decimal BalanceGrams);
    public sealed class WalletApplicationService(IGoldWalletRepository wallets)
    {
        public async Task<WalletBalanceDto> GetBalanceAsync(Guid userId, CancellationToken ct = default)
        {
            if (userId == Guid.Empty) throw new ValidationException("UserId is required.");
            var wallet = await wallets.GetOrCreateAsync(userId, ct);
            return new WalletBalanceDto(wallet.UserId, wallet.Balance.Grams);
        }
    }
}

