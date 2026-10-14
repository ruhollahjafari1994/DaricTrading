namespace DaricTrading.Application.Orders.Commands;

/// <summary>شناسه سفارش هدف را برای اجرای درخواست لغو حمل می‌کند.</summary>
/// <param name="OrderId">شناسه سفارشی که باید لغو شود.</param>
public sealed record CancelOrderCommand(Guid OrderId);
