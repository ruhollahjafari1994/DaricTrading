using DaricTrading.API.Contracts;
using DaricTrading.Application.Orders;
using Microsoft.AspNetCore.Mvc;

namespace DaricTrading.API.Controllers;

/// <summary>Creates and manages gold orders.</summary>
[ApiController]
[Route("api/v1/orders")]
[Produces("application/json")]
public sealed class OrdersController(BuyOrderHandler buyOrder, SellOrderHandler sellOrder, GetOrderHandler getOrder, SearchOrdersHandler searchOrders, UpdateOrderTagsHandler updateOrderTags, CancelOrderHandler cancelOrder) : ControllerBase
{
    /// <summary>Creates and settles a buy order.</summary>
    /// <param name="request">Buy order details.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="201">The settled buy order was created.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="422">A business rule prevented settlement.</response>
    [HttpPost("buy", Name = nameof(Buy))]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderResponse>> Buy(CreateBuyOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(request.UserId, request.WeightGrams, request.UnitPrice, request.Fee, request.Tags);
        var order = await buyOrder.HandleAsync(command, cancellationToken);
        var response = OrderResponse.From(order);
        return CreatedAtRoute(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>Creates and settles a sell order.</summary>
    /// <param name="request">Sell order details.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="201">The settled sell order was created.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="422">A business rule prevented settlement.</response>
    [HttpPost("sell", Name = nameof(Sell))]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderResponse>> Sell(CreateSellOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(request.UserId, request.WeightGrams, request.UnitPrice, request.Fee, request.Tags);
        var order = await sellOrder.HandleAsync(command, cancellationToken);
        var response = OrderResponse.From(order);
        return CreatedAtRoute(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>Gets an order by its identifier.</summary>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">The order was found.</response>
    /// <response code="404">No order has the supplied identifier.</response>
    [HttpGet("{id:guid}", Name = nameof(GetById))]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await getOrder.HandleAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(OrderResponse.From(order));
    }

    /// <summary>Searches settled orders using the supplied filters.</summary>
    /// <param name="query">Search filters.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">The matching orders.</response>
    /// <response code="400">The filters failed validation.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<OrderResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> Search([FromQuery] SearchOrdersQueryParameters query, CancellationToken cancellationToken)
    {
        var search = new OrderSearchQuery(query.UserId, query.Tag, query.From, query.To, query.Type);
        var result = await searchOrders.HandleAsync(search, cancellationToken);
        return Ok(result.Select(OrderResponse.From).ToArray());
    }

    /// <summary>Replaces the tags and note on a settled order.</summary>
    /// <param name="id">The order identifier.</param>
    /// <param name="request">New tags and optional note.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">The order metadata was updated.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="404">No order has the supplied identifier.</response>
    /// <response code="422">The order status does not allow metadata updates.</response>
    [HttpPatch("{id:guid}/tags")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderResponse>> UpdateTags(Guid id, UpdateOrderTagsRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateOrderTagsCommand(id, request.Tags, request.Note);
        var result = await updateOrderTags.HandleAsync(command, cancellationToken);
        return Ok(OrderResponse.From(result));
    }

    /// <summary>Cancels a pending order.</summary>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">The order was cancelled.</response>
    /// <response code="404">No order has the supplied identifier.</response>
    /// <response code="422">The order is not pending.</response>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderResponse>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var order = await cancelOrder.HandleAsync(id, cancellationToken);
        return Ok(OrderResponse.From(order));
    }
}


