using DaricTrading.API;
using DaricTrading.API.Errors;
using DaricTrading.Application;
using DaricTrading.Infrastructure;
using DaricTrading.Infrastructure.Persistence.PostgreSql;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
var usePostgreSql = string.Equals(builder.Configuration["Persistence:Provider"], "PostgreSql", StringComparison.OrdinalIgnoreCase);
if (usePostgreSql)
{
    var connectionString = builder.Configuration.GetConnectionString("Trading")
        ?? throw new InvalidOperationException("ConnectionStrings:Trading is required for PostgreSQL persistence.");
    builder.Services.AddPostgreSqlPersistence(connectionString);
}
else
{
    builder.Services.AddInMemoryTradingPersistence();
}

builder.Services.AddDaricTradingApplication();
builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
    options.SchemaFilter<TradingExamplesSchemaFilter>();
});

var app = builder.Build();
if (usePostgreSql) await app.Services.ApplyTradingMigrationsAsync();
app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();

/// <summary>نوع عمومی برنامه را در دسترس WebApplicationFactory برای آزمون‌های یکپارچگی قرار می‌دهد.</summary>
public partial class Program { }
