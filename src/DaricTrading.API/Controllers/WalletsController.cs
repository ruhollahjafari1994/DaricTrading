using DaricTrading.Application.Wallets;
using Microsoft.AspNetCore.Mvc;
namespace DaricTrading.API.Controllers;
[ApiController]
[Route("api/wallets")]
public sealed class WalletsController(WalletApplicationService service) : ControllerBase
{
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<WalletBalanceDto>> Get(Guid userId, CancellationToken cancellationToken) => Ok(await service.GetBalanceAsync(userId, cancellationToken));
}
