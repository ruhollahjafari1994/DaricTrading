namespace DaricTrading.Application.Orders.Queries;

/// <summary>شناسه سفارش را برای بازیابی جزئیات همان سفارش حمل می‌کند.</summary>
/// <param name="OrderId">شناسه سفارش مورد بازیابی.</param>
public sealed record GetOrderQuery(Guid OrderId);
