using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using DaricTrading.Application.Orders;
using DaricTrading.Application.Orders.Commands;
using DaricTrading.Application.Orders.Queries;
using DaricTrading.Application.Validation;
using DaricTrading.Application.Wallets;

namespace DaricTrading.Application;

/// <summary>سرویس‌های موردکار و اعتبارسنج‌های لایه کاربردی را در تزریق وابستگی ثبت می‌کند.</summary>
public static class ApplicationRegistration
{
    /// <summary>تمام موردکارها و اعتبارسنج‌های اپلیکیشن را برای استفاده API ثبت می‌کند.</summary>
    public static IServiceCollection AddDaricTradingApplication(this IServiceCollection services)
    {
        services.AddScoped<BuyOrderHandler>();
        services.AddScoped<SellOrderHandler>();
        services.AddScoped<GetOrderHandler>();
        services.AddScoped<SearchOrdersHandler>();
        services.AddScoped<UpdateOrderTagsHandler>();
        services.AddScoped<CancelOrderHandler>();
        services.AddScoped<GetWalletBalanceHandler>();
        services.AddScoped<IValidator<CreateBuyOrderCommand>, CreateBuyOrderCommandValidator>();
        services.AddScoped<IValidator<CreateSellOrderCommand>, CreateSellOrderCommandValidator>();
        services.AddScoped<IValidator<SearchOrdersQuery>, SearchOrdersQueryValidator>();
        services.AddScoped<IValidator<UpdateOrderTagsCommand>, UpdateOrderTagsCommandValidator>();
        services.AddScoped<IValidator<CancelOrderCommand>, CancelOrderCommandValidator>();
        services.AddScoped<IValidator<GetOrderQuery>, GetOrderQueryValidator>();
        services.AddScoped<IValidator<GetWalletBalanceQuery>, GetWalletBalanceQueryValidator>();
        return services;
    }
}
