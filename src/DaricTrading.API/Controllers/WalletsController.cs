using DaricTrading.API.Contracts;
using DaricTrading.Application.Wallets;
using Microsoft.AspNetCore.Mvc;

namespace DaricTrading.API.Controllers;

/// <summary>درخواست‌های دریافت موجودی کیف پول طلای کاربران را مدیریت می‌کند.</summary>
[ApiController]
[Route("api/v1/wallets")]
[Produces("application/json")]
public sealed class WalletsController(GetWalletBalanceHandler wallets) : ControllerBase
{
    /// <summary>موجودی فعلی طلای کاربر را برمی‌گرداند.</summary>
    [HttpGet("{userId:guid}/balance")]
    [ProducesResponseType<WalletBalanceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WalletBalanceResponse>> GetBalance(Guid userId, CancellationToken cancellationToken)
    {
        var balance = await wallets.HandleAsync(new GetWalletBalanceQuery(userId), cancellationToken);
        return Ok(WalletBalanceResponse.From(balance));
    }
}
