using FluentValidation;
using DaricTrading.Application.Orders;
using DaricTrading.Domain;

namespace DaricTrading.Application.Validation;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.WeightGrams).GreaterThan(0).Must(value => decimal.Round(value, 3) == value).WithMessage("Weight must be positive and have at most 3 decimal places.");
        RuleFor(command => command.UnitPrice).GreaterThanOrEqualTo(0).Must(value => decimal.Round(value, 0) == value).WithMessage("Unit price must be a non-negative whole IRR amount.");
        RuleFor(command => command.Fee).GreaterThanOrEqualTo(0).Must(value => decimal.Round(value, 0) == value).WithMessage("Fee must be a non-negative whole IRR amount.");
        RuleForEach(command => command.Tags ?? Array.Empty<string>()).Must(IsAllowedTag).WithMessage("Tag is not supported.");
    }
    internal static bool IsAllowedTag(string value)
    {
        try { _ = new Tag(value); return true; }
        catch (ArgumentException) { return false; }
    }
}

public sealed class OrderSearchQueryValidator : AbstractValidator<OrderSearchQuery>
{
    public OrderSearchQueryValidator()
    {
        RuleFor(query => query.UserId).NotEmpty();
        RuleFor(query => query).Must(query => !query.From.HasValue || !query.To.HasValue || query.From <= query.To).WithMessage("From must be earlier than or equal to To.");
        RuleFor(query => query.Tag).Must(value => value is null || CreateOrderCommandValidator.IsAllowedTag(value)).WithMessage("Tag is not supported.");
        RuleFor(query => query.Type).Must(value => value is null || value.Equals("Buy", StringComparison.OrdinalIgnoreCase) || value.Equals("Sell", StringComparison.OrdinalIgnoreCase)).WithMessage("Type must be Buy or Sell.");
    }
}

public sealed class UpdateOrderTagsCommandValidator : AbstractValidator<UpdateOrderTagsCommand>
{
    public UpdateOrderTagsCommandValidator()
    {
        RuleFor(command => command.OrderId).NotEmpty();
        RuleFor(command => command.Tags).NotNull();
        RuleForEach(command => command.Tags).Must(CreateOrderCommandValidator.IsAllowedTag).WithMessage("Tag is not supported.");
        RuleFor(command => command.Note).MaximumLength(500);
    }
}

