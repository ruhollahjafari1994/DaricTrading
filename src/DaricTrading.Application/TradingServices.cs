using DaricTrading.Application.Common;
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
        public static OrderDto From(GoldOrder order) => new(order.Id, order.UserId, order.Type.ToString(), order.Weight.Grams, order.UnitPrice.Amount, order.Fee.Amount, order.TotalAmount.Amount, order.TotalAmount.Currency, order.Tags.Select(tag => tag.Value).ToArray(), order.UserNote, order.Status.ToString(), order.CreatedAt);
    }

    public sealed class BuyOrderHandler(IGoldOrderRepository orders, IGoldWalletRepository wallets, IDomainEventDispatcher eventDispatcher, IClock clock, IValidator<CreateOrderCommand> validator)
    {
        public Task<OrderDto> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken = default) => DomainExceptionBoundary.ExecuteAsync(async () =>
        {
            await validator.ValidateAndThrowAsync(command, cancellationToken);
            var weight = GoldWeight.FromGrams(command.WeightGrams);
            var tags = (command.Tags ?? Array.Empty<string>()).Select(value => new Tag(value)).ToArray();
            var order = GoldOrder.Buy(command.UserId, weight, new Money(command.UnitPrice), new Money(command.Fee), tags, clock.UtcNow);
            var wallet = await wallets.GetOrCreateAsync(command.UserId, cancellationToken);
            wallet.Credit(weight);
            await wallets.SaveAsync(wallet, cancellationToken);
            await orders.AddAsync(order, cancellationToken);
            await eventDispatcher.DispatchAsync(order.DequeueDomainEvents(), cancellationToken);
            return OrderDto.From(order);
        });
    }

    public sealed class SellOrderHandler(IGoldOrderRepository orders, IGoldWalletRepository wallets, IDomainEventDispatcher eventDispatcher, IClock clock, IValidator<CreateOrderCommand> validator)
    {
        public Task<OrderDto> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken = default) => DomainExceptionBoundary.ExecuteAsync(async () =>
        {
            await validator.ValidateAndThrowAsync(command, cancellationToken);
            var weight = GoldWeight.FromGrams(command.WeightGrams);
            var tags = (command.Tags ?? Array.Empty<string>()).Select(value => new Tag(value)).ToArray();
            var order = GoldOrder.Sell(command.UserId, weight, new Money(command.UnitPrice), new Money(command.Fee), tags, clock.UtcNow);
            var wallet = await wallets.GetOrCreateAsync(command.UserId, cancellationToken);
            wallet.Debit(weight);
            await wallets.SaveAsync(wallet, cancellationToken);
            await orders.AddAsync(order, cancellationToken);
            await eventDispatcher.DispatchAsync(order.DequeueDomainEvents(), cancellationToken);
            return OrderDto.From(order);
        });
    }

    public sealed class GetOrderHandler(IGoldOrderRepository orders)
    {
        public async Task<OrderDto?> HandleAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var order = await orders.GetAsync(id, cancellationToken);
            return order is null ? null : OrderDto.From(order);
        }
    }

    public sealed class SearchOrdersHandler(IGoldOrderRepository orders, IValidator<OrderSearchQuery> validator)
    {
        public async Task<IReadOnlyList<OrderDto>> HandleAsync(OrderSearchQuery query, CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(query, cancellationToken);
            OrderType? type = string.IsNullOrWhiteSpace(query.Type) ? null : Enum.Parse<OrderType>(query.Type, true);
            var result = await orders.SearchAsync(query.UserId, query.Tag, query.From, query.To, type, cancellationToken);
            return result.Select(OrderDto.From).ToArray();
        }
    }

    public sealed class UpdateOrderTagsHandler(IGoldOrderRepository orders, IValidator<UpdateOrderTagsCommand> validator)
    {
        public Task<OrderDto> HandleAsync(UpdateOrderTagsCommand command, CancellationToken cancellationToken = default) => DomainExceptionBoundary.ExecuteAsync(async () =>
        {
            await validator.ValidateAndThrowAsync(command, cancellationToken);
            var order = await orders.GetAsync(command.OrderId, cancellationToken) ?? throw new NotFoundException("Order", command.OrderId);
            order.UpdateTags(command.Tags.Select(value => new Tag(value)), command.Note);
            await orders.UpdateAsync(order, cancellationToken);
            return OrderDto.From(order);
        });
    }

    public sealed class CancelOrderHandler(IGoldOrderRepository orders, IDomainEventDispatcher eventDispatcher, IClock clock)
    {
        public Task<OrderDto> HandleAsync(Guid id, CancellationToken cancellationToken = default) => DomainExceptionBoundary.ExecuteAsync(async () =>
        {
            var order = await orders.GetAsync(id, cancellationToken) ?? throw new NotFoundException("Order", id);
            order.Cancel(clock.UtcNow);
            await orders.UpdateAsync(order, cancellationToken);
            await eventDispatcher.DispatchAsync(order.DequeueDomainEvents(), cancellationToken);
            return OrderDto.From(order);
        });
    }

}
namespace DaricTrading.Application.Wallets
{
    public sealed record WalletBalanceDto(Guid UserId, decimal BalanceGrams);
    public sealed class WalletApplicationService(IGoldWalletRepository wallets)
    {
        public async Task<WalletBalanceDto> GetBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty) throw new ValidationException("UserId is required.");
            var wallet = await wallets.GetOrCreateAsync(userId, cancellationToken);
            return new WalletBalanceDto(wallet.UserId, wallet.Balance.Grams);
        }
    }
}

