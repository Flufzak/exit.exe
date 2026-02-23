using Exit.exe.Application.Features.Games.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exit.exe.Web.Controllers;

[ApiController]
[Route("api/games")]
[Authorize] // Not logged in => 401
public sealed class GamesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GameListItem>>> Get(CancellationToken ct)
    {
        var result = await sender.Send(new GetGamesQuery(), ct);
        return Ok(result);
    }
}