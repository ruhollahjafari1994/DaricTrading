using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DaricTrading.API;

public sealed class TradingExamplesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(Contracts.CreateBuyOrderRequest) || context.Type == typeof(Contracts.CreateSellOrderRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["userId"] = new OpenApiString("10000000-0000-0000-0000-000000000001"),
                ["weightGrams"] = new OpenApiDouble(1.250),
                ["unitPrice"] = new OpenApiDouble(3_500_000),
                ["fee"] = new OpenApiDouble(5_000),
                ["tags"] = new OpenApiArray { new OpenApiString("پس‌انداز_بلندمدت") }
            };
        }
        else if (context.Type == typeof(Contracts.UpdateOrderTagsRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["tags"] = new OpenApiArray { new OpenApiString("نوسان‌گیری_روزانه") },
                ["note"] = new OpenApiString("Portfolio rebalance")
            };
        }
    }
}
