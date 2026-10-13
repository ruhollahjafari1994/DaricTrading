using DaricTrading.Application.Wallets;

namespace DaricTrading.API.Contracts;

/// <summary>Request data for a market buy order.</summary>
public sealed record CreateBuyOrderRequest(Guid UserId, decimal WeightGrams, decimal UnitPrice, decimal Fee, string[]? Tags);
/// <summary>Request data for a market sell order.</summary>
public sealed record CreateSellOrderRequest(Guid UserId, decimal WeightGrams, decimal UnitPrice, decimal Fee, string[]? Tags);
/// <summary>Replacement tags and optional note for an order.</summary>
public sealed record UpdateOrderTagsRequest(string[] Tags, string? Note);
/// <summary>Order search filters. Dates are inclusive and should be provided as ISO 8601 timestamps.</summary>
public sealed record SearchOrdersQueryParameters(Guid UserId, string? Tag, DateTimeOffset? From, DateTimeOffset? To, string? OrderType);
/// <summary>HTTP representation of an order; contains no domain types.</summary>
public sealed record OrderResponse(Guid Id, Guid UserId, string Type, decimal WeightGrams, decimal UnitPrice, decimal Fee, decimal TotalAmount, string Currency, IReadOnlyList<string> Tags, string? UserNote, string Status, DateTimeOffset CreatedAt)
{
    public static OrderResponse From(DaricTrading.Application.Orders.OrderDto order) => new(order.Id, order.UserId, order.Type, order.WeightGrams, order.UnitPrice, order.Fee, order.TotalAmount, order.Currency, order.Tags.ToArray(), order.Note, order.Status, order.CreatedAt);
}
/// <summary>Public wallet balance response.</summary>
public sealed record WalletBalanceResponse(Guid UserId, decimal BalanceGrams)
{
    public static WalletBalanceResponse From(WalletBalanceDto balance) => new(balance.UserId, balance.BalanceGrams);
}

