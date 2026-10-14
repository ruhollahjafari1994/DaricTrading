using DaricTrading.Application.Orders.Commands;
using DaricTrading.Domain;
using FluentValidation;

namespace DaricTrading.Application.Validation;

/// <summary>قواعد مشترک شناسه، وزن مثبت با سه رقم اعشار، مبلغ با دو رقم اعشار و برچسب مجاز را اعمال می‌کند.</summary>
public abstract class CreateOrderCommandValidator<TCommand> : AbstractValidator<TCommand>
    where TCommand : CreateOrderCommand
{
    /// <summary>قواعد مشترک شناسه کاربر، وزن، قیمت، کارمزد و برچسب سفارش را ثبت می‌کند.</summary>
    protected CreateOrderCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.WeightGrams)
            .InclusiveBetween(0.001m, GoldWeight.MaximumSupportedGrams)
            .Must(value => decimal.Round(value, 3) == value)
            .WithMessage("Weight must be positive, supported by storage, and have at most 3 decimal places.");
        RuleFor(command => command.UnitPrice)
            .InclusiveBetween(0m, Money.MaximumSupportedAmount)
            .Must(value => decimal.Round(value, 2) == value)
            .WithMessage("Unit price must fit the supported range with at most two decimal places.");
        RuleFor(command => command.Fee)
            .InclusiveBetween(0m, Money.MaximumSupportedAmount)
            .Must(value => decimal.Round(value, 2) == value)
            .WithMessage("Fee must fit the supported range with at most two decimal places.");
        RuleForEach(command => command.Tags!).Must(TagValidation.IsAllowed).WithMessage("Tag is not supported.");
    }
}
