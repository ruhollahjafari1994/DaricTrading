using DaricTrading.Domain;

namespace DaricTrading.Application.Validation;

/// <summary>مجاز بودن مقدار برچسب را با قواعد دامنه بررسی می‌کند.</summary>
internal static class TagValidation
{
    /// <summary>بررسی می‌کند مقدار برچسب در فهرست مقادیر مجاز باشد.</summary>
    internal static bool IsAllowed(string value)
    {
        try
        {
            _ = new Tag(value);
            return true;
        }
        catch (DomainRuleViolationException)
        {
            return false;
        }
    }
}
