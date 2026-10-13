using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using DaricTrading.Application.Orders;
using DaricTrading.Application.Validation;
using DaricTrading.Application.Wallets;

namespace DaricTrading.Application;

public static class ApplicationRegistration
{
    public static IServiceCollection AddDaricTradingApplication(this IServiceCollection services)
    {
        services.AddScoped<BuyOrderHandler>();
        services.AddScoped<SellOrderHandler>();
        services.AddScoped<GetOrderHandler>();
        services.AddScoped<SearchOrdersHandler>();
        services.AddScoped<UpdateOrderTagsHandler>();
        services.AddScoped<CancelOrderHandler>();
        services.AddScoped<WalletApplicationService>();
        services.AddScoped<IValidator<CreateOrderCommand>, CreateOrderCommandValidator>();
        services.AddScoped<IValidator<OrderSearchQuery>, OrderSearchQueryValidator>();
        services.AddScoped<IValidator<UpdateOrderTagsCommand>, UpdateOrderTagsCommandValidator>();
        return services;
    }
}
