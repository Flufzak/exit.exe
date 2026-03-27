using System.Security.Claims;
using Exit.exe.Application.Features.Profile.DTOs;
using Exit.exe.Application.Features.Profile.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exit.exe.Web.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public sealed class ProfileController(ISender sender) : ControllerBase
{
    /// <summary>
    /// GET /api/profile/stats — Get gameplay statistics for the current user.
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<ProfileStatsDto>> Stats(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in claims.");

        var query = new GetProfileStatsQuery(userId);
        var result = await sender.Send(query, ct);
        return Ok(result);
    }
}
