using DaricTrading.API.Errors;
using DaricTrading.Application.Orders;
using DaricTrading.Application.Validation;
using DaricTrading.Application.Wallets;
using DaricTrading.Domain;
using DaricTrading.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSingleton<IGoldOrderRepository, InMemoryGoldOrderRepository>();
builder.Services.AddSingleton<IGoldWalletRepository, InMemoryGoldWalletRepository>();
builder.Services.AddScoped<BuyOrderHandler>();
builder.Services.AddScoped<SellOrderHandler>();
builder.Services.AddScoped<GetOrderHandler>();
builder.Services.AddScoped<SearchOrdersHandler>();
builder.Services.AddScoped<UpdateOrderTagsHandler>();
builder.Services.AddScoped<CancelOrderHandler>();
builder.Services.AddScoped<WalletApplicationService>();
builder.Services.AddScoped<IValidator<CreateOrderCommand>, CreateOrderCommandValidator>();
builder.Services.AddScoped<IValidator<OrderSearchQuery>, OrderSearchQueryValidator>();
builder.Services.AddScoped<IValidator<UpdateOrderTagsCommand>, UpdateOrderTagsCommandValidator>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
});

var app = builder.Build();
app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();

public partial class Program { }

