using System.Text.Json;
using Exit.exe.Application.Contracts;
using Exit.exe.Application.Features.Sessions.DTOs;
using MediatR;

namespace Exit.exe.Application.Features.Sessions.Queries;

public sealed record GetSessionQuery(Guid SessionId, string UserId) : IRequest<SessionDto>;

public sealed class GetSessionQueryHandler(
    ISessionRepository sessionRepository) : IRequestHandler<GetSessionQuery, SessionDto>
{
    public async Task<SessionDto> Handle(GetSessionQuery request, CancellationToken cancellationToken)
    {
        var data = await sessionRepository.GetWithPuzzleAsync(request.SessionId, request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Session '{request.SessionId}' not found.");

        var payload = JsonSerializer.Deserialize<HangmanPayload>(data.PuzzlePayload)
            ?? throw new InvalidOperationException("Invalid puzzle payload.");

        var guessedLetters = HangmanHelper.ParseGuessedLetters(data.Session.GuessedLetters);
        var maskedWord = HangmanHelper.MaskWord(payload.Word, guessedLetters);

        return new SessionDto(
            data.Session.Id,
            data.PuzzleGameType,
            maskedWord,
            data.Session.AttemptsLeft,
            guessedLetters,
            data.Session.Status.ToString());
    }
}
