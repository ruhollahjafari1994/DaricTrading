namespace DaricTrading.Application.Common;

/// <summary>قرارداد ثبت یکپارچه تغییرات یک موردکار را تعریف می‌کند.</summary>
public interface IUnitOfWork
{
    /// <summary>تغییرات مخزن‌ها را در یک commit ثبت می‌کند تا ترتیب عملیات مالی حفظ شود.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
