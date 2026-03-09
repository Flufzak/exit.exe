using System.Text.Json;
using Exit.exe.Application.Features.Sessions.DTOs;
using Exit.exe.Repository.Data.App;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exit.exe.Application.Features.Sessions.Queries;

public sealed record GetSessionQuery(Guid SessionId, string UserId) : IRequest<SessionDto>;

public sealed class GetSessionQueryHandler(AppDbContext db) : IRequestHandler<GetSessionQuery, SessionDto>
{
    public async Task<SessionDto> Handle(GetSessionQuery request, CancellationToken cancellationToken)
    {
        var session = await db.GameSessions
            .Include(s => s.Puzzle)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.UserId == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Session '{request.SessionId}' not found.");

        var payload = JsonSerializer.Deserialize<HangmanPayload>(session.Puzzle.Payload)
            ?? throw new InvalidOperationException("Invalid puzzle payload.");

        var guessedLetters = HangmanHelper.ParseGuessedLetters(session.GuessedLetters);
        var maskedWord = HangmanHelper.MaskWord(payload.Word, guessedLetters);

        return new SessionDto(
            session.Id,
            session.Puzzle.GameType,
            maskedWord,
            session.AttemptsLeft,
            guessedLetters,
            session.Status.ToString());
    }
}
