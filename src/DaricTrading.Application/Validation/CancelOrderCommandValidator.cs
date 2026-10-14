using DaricTrading.Application.Orders.Commands;
using FluentValidation;

namespace DaricTrading.Application.Validation;

/// <summary>خالی نبودن شناسه سفارش را پیش از اجرای لغو کنترل می‌کند.</summary>
public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    /// <summary>قانون ضروری بودن شناسه سفارش برای لغو را ثبت می‌کند.</summary>
    public CancelOrderCommandValidator()
    {
        RuleFor(command => command.OrderId).NotEmpty();
    }
}
