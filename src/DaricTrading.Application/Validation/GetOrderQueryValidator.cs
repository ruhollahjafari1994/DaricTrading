using DaricTrading.Application.Orders.Queries;
using FluentValidation;

namespace DaricTrading.Application.Validation;

/// <summary>شناسه سفارش را پیش از ارسال پرس‌وجو به مخزن الزامی می‌کند.</summary>
public sealed class GetOrderQueryValidator : AbstractValidator<GetOrderQuery>
{
    /// <summary>قانون ضروری بودن شناسه سفارش برای بازیابی آن را ثبت می‌کند.</summary>
    public GetOrderQueryValidator()
    {
        RuleFor(query => query.OrderId).NotEmpty();
    }
}
