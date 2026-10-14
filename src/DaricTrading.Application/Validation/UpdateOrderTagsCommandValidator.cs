using DaricTrading.Application.Orders.Commands;
using DaricTrading.Domain;
using FluentValidation;

namespace DaricTrading.Application.Validation;

/// <summary>شناسه سفارش، وجود فهرست برچسب، مجاز بودن هر برچسب و سقف ۵۰۰ کاراکتری یادداشت را بررسی می‌کند.</summary>
public sealed class UpdateOrderTagsCommandValidator : AbstractValidator<UpdateOrderTagsCommand>
{
    /// <summary>محدودیت تعداد برچسب‌ها، مقادیر مجاز و طول یادداشت را ثبت می‌کند.</summary>
    public UpdateOrderTagsCommandValidator()
    {
        RuleFor(command => command.OrderId).NotEmpty();
        RuleFor(command => command.Tags).NotNull();
        RuleForEach(command => command.Tags).Must(TagValidation.IsAllowed).WithMessage("Tag is not supported.");
        RuleFor(command => command.Note).MaximumLength(500);
    }
}
