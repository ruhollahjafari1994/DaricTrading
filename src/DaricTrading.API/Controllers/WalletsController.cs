using DaricTrading.API.Contracts;
using DaricTrading.Application.Wallets;
using Microsoft.AspNetCore.Mvc;

namespace DaricTrading.API.Controllers;

/// <summary>Reads gold wallet balances.</summary>
[ApiController]
[Route("api/v1/wallets")]
[Produces("application/json")]
public sealed class WalletsController(WalletApplicationService wallets) : ControllerBase
{
    /// <summary>Gets a user's current gold balance.</summary>
    /// <param name="userId">The wallet owner.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">The wallet balance.</response>
    /// <response code="400">The user identifier is invalid.</response>
    [HttpGet("{userId:guid}/balance")]
    [ProducesResponseType<WalletBalanceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WalletBalanceResponse>> GetBalance(Guid userId, CancellationToken cancellationToken)
    {
        var balance = await wallets.GetBalanceAsync(userId, cancellationToken);
        return Ok(WalletBalanceResponse.From(balance));
    }
}
