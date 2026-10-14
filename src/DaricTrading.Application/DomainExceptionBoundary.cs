namespace DaricTrading.Application.Common;

/// <summary>خطاهای دامنه را در مرز لایه کاربردی به خطاهای مناسب این لایه نگاشت می‌کند.</summary>
public static class DomainExceptionBoundary
{
    public static async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            return await operation();
        }
        catch (DaricTrading.Domain.ConcurrencyConflictException exception)
        {
            throw new OptimisticConcurrencyException(exception.Message);
        }
        catch (DaricTrading.Domain.DomainRuleViolationException exception)
        {
            throw new BusinessRuleViolationException(exception.Message);
        }
    }
}
