using DaricTrading.Application.Orders.Commands;

namespace DaricTrading.Application.Validation;

/// <summary>فرمان خرید را با قواعد پایه دقت و محدوده فیلدهای مالی سفارش ارزیابی می‌کند.</summary>
public sealed class CreateBuyOrderCommandValidator
    : CreateOrderCommandValidator<CreateBuyOrderCommand>
{
}
