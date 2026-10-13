namespace DaricTrading.Application.Common;

public sealed class BusinessRuleViolationException(string message) : Exception(message);
public sealed class OptimisticConcurrencyException(string message) : Exception(message);

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
