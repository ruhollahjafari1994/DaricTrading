using DaricTrading.API.Contracts;
using DaricTrading.Application.Orders;
using DaricTrading.Application.Orders.Commands;
using DaricTrading.Application.Orders.Queries;
using Microsoft.AspNetCore.Mvc;

namespace DaricTrading.API.Controllers;

/// <summary>درخواست‌های HTTP مربوط به ثبت، جست‌وجو و مدیریت سفارش‌های طلا را به لایه کاربردی واگذار می‌کند.</summary>
[ApiController]
[Route("api/v1/orders")]
[Produces("application/json")]
public sealed class OrdersController(BuyOrderHandler buyOrder, SellOrderHandler sellOrder, GetOrderHandler getOrder, SearchOrdersHandler searchOrders, UpdateOrderTagsHandler updateOrderTags, CancelOrderHandler cancelOrder) : ControllerBase
{
    /// <summary>سفارش خرید طلا را ثبت و تسویه می‌کند.</summary>
    [HttpPost("buy", Name = nameof(Buy))]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderResponse>> Buy(CreateBuyOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateBuyOrderCommand(request.UserId, request.WeightGrams, request.UnitPrice, request.Fee, request.Tags);
        var order = await buyOrder.HandleAsync(command, cancellationToken);
        var response = OrderResponse.From(order);
        return CreatedAtRoute(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>سفارش فروش طلا را ثبت و تسویه می‌کند.</summary>
    [HttpPost("sell", Name = nameof(Sell))]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderResponse>> Sell(CreateSellOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateSellOrderCommand(request.UserId, request.WeightGrams, request.UnitPrice, request.Fee, request.Tags);
        var order = await sellOrder.HandleAsync(command, cancellationToken);
        var response = OrderResponse.From(order);
        return CreatedAtRoute(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>جزئیات سفارش را با شناسه آن برمی‌گرداند.</summary>
    [HttpGet("{id:guid}", Name = nameof(GetById))]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await getOrder.HandleAsync(new GetOrderQuery(id), cancellationToken);
        return Ok(OrderResponse.From(order));
    }

    /// <summary>سفارش‌های تسویه‌شده را با فیلترهای درخواست جست‌وجو می‌کند.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<OrderResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> Search([FromQuery] SearchOrdersQueryParameters query, CancellationToken cancellationToken)
    {
        var search = new SearchOrdersQuery(query.UserId, query.Tag, query.From, query.To, query.OrderType);
        var result = await searchOrders.HandleAsync(search, cancellationToken);
        return Ok(result.Select(OrderResponse.From).ToArray());
    }

    /// <summary>برچسب‌ها و یادداشت سفارش تسویه‌شده را بدون تغییر اطلاعات مالی به‌روزرسانی می‌کند.</summary>
    [HttpPatch("{id:guid}/tags")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTags(Guid id, UpdateOrderTagsRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateOrderTagsCommand(id, request.Tags, request.Note);
        await updateOrderTags.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    /// <summary>سفارش در انتظار را لغو می‌کند.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await cancelOrder.HandleAsync(new CancelOrderCommand(id), cancellationToken);
        return NoContent();
    }
}

