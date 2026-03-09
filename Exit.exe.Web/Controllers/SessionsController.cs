using System.Security.Claims;
using Exit.exe.Application.Features.Sessions.Commands;
using Exit.exe.Application.Features.Sessions.DTOs;
using Exit.exe.Application.Features.Sessions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exit.exe.Web.Controllers;

[ApiController]
[Route("api/sessions")]
[Authorize]
public sealed class SessionsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// POST /api/sessions — Start a new game session.
    /// Body: { "gameType": "hangman" }
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SessionDto>> Start(
        [FromBody] StartSessionRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var command = new StartSessionCommand(request.GameType, userId);
        var result = await sender.Send(command, ct);
        return CreatedAtAction(nameof(Get), new { id = result.SessionId }, result);
    }

    /// <summary>
    /// GET /api/sessions/{id} — Get current session state.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SessionDto>> Get(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var query = new GetSessionQuery(id, userId);
        var result = await sender.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/sessions/{id}/guess — Submit a letter guess.
    /// Body: { "letter": "A" }
    /// </summary>
    [HttpPost("{id:guid}/guess")]
    public async Task<ActionResult<GuessResultDto>> Guess(
        Guid id,
        [FromBody] GuessRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var command = new SubmitGuessCommand(id, request.Letter, userId);
        var result = await sender.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/sessions/{id}/hint — Request a hint.
    /// </summary>
    [HttpPost("{id:guid}/hint")]
    public async Task<ActionResult<HintResultDto>> Hint(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var command = new RequestHintCommand(id, userId);
        var result = await sender.Send(command, ct);
        return Ok(result);
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in claims.");
    }
}

public sealed record StartSessionRequest(string GameType);
public sealed record GuessRequest(string Letter);
