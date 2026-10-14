using DaricTrading.Application.Orders.Queries;
using DaricTrading.Domain;
using FluentValidation;

namespace DaricTrading.Application.Validation;

/// <summary>کاربر، ترتیب ابتدا و انتهای تاریخ، برچسب مجاز و نوع Buy/Sell را بررسی می‌کند.</summary>
public sealed class SearchOrdersQueryValidator : AbstractValidator<SearchOrdersQuery>
{
    /// <summary>قواعد شناسه کاربر و بازه زمانی فیلتر جست‌وجو را ثبت می‌کند.</summary>
    public SearchOrdersQueryValidator()
    {
        RuleFor(query => query.UserId).NotEmpty();
        RuleFor(query => query).Must(query => !query.From.HasValue || !query.To.HasValue || query.From <= query.To).WithMessage("From must be earlier than or equal to To.");
        RuleFor(query => query.Tag).Must(value => value is null || TagValidation.IsAllowed(value)).WithMessage("Tag is not supported.");
        RuleFor(query => query.Type).Must(value => value is null || value.Equals("Buy", StringComparison.OrdinalIgnoreCase) || value.Equals("Sell", StringComparison.OrdinalIgnoreCase)).WithMessage("Type must be Buy or Sell.");
    }
}
