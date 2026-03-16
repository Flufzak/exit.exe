using System.Text.Json;
using Exit.exe.Application.Contracts;
using Exit.exe.Application.Features.Sessions.DTOs;
using Exit.exe.Domain.Entities;
using MediatR;

namespace Exit.exe.Application.Features.Sessions.Commands;

public sealed record StartSessionCommand(string GameType, string UserId) : IRequest<SessionDto>;

public sealed class StartSessionCommandHandler(
    IPuzzleRepository puzzleRepository,
    ISessionRepository sessionRepository) : IRequestHandler<StartSessionCommand, SessionDto>
{
    public async Task<SessionDto> Handle(StartSessionCommand request, CancellationToken cancellationToken)
    {
        // Pick a random puzzle of the requested game type
        var puzzles = await puzzleRepository.GetByGameTypeAsync(request.GameType, cancellationToken);

        if (puzzles.Count == 0)
            throw new InvalidOperationException($"No puzzles found for game type '{request.GameType}'.");

        var puzzle = puzzles[Random.Shared.Next(puzzles.Count)];

        var payload = JsonSerializer.Deserialize<HangmanPayload>(puzzle.Payload)
            ?? throw new InvalidOperationException("Invalid puzzle payload.");

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            PuzzleId = puzzle.Id,
            Status = SessionStatus.InProgress,
            GuessedLetters = string.Empty,
            AttemptsLeft = payload.MaxAttempts,
            HintsUsed = 0,
            StartedAtUtc = DateTime.UtcNow
        };

        sessionRepository.Add(session);
        await sessionRepository.SaveChangesAsync(cancellationToken);

        var maskedWord = HangmanHelper.MaskWord(payload.Word, []);

        return new SessionDto(
            session.Id,
            request.GameType,
            maskedWord,
            session.AttemptsLeft,
            [],
            session.Status.ToString());
    }
}
