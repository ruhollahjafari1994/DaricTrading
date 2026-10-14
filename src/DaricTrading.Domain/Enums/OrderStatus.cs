namespace DaricTrading.Domain;

/// <summary>وضعیت چرخه عمر سفارش را مشخص می‌کند.</summary>
public enum OrderStatus
{
    /// <summary>سفارش هنوز تسویه یا لغو نشده است.</summary>
    Pending,
    /// <summary>سفارش با موفقیت تسویه شده است.</summary>
    Settled,
    /// <summary>سفارش لغو شده است.</summary>
    Cancelled
}
