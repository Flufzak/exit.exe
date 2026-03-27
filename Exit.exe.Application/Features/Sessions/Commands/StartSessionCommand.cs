using System.Text.Json;
using Exit.exe.Application.Contracts;
using Exit.exe.Application.Features.Sessions.DTOs;
using Exit.exe.Domain.Entities;
using MediatR;

namespace Exit.exe.Application.Features.Sessions.Commands;

public sealed record StartSessionCommand(string GameType, string UserId) : IRequest<SessionDto>;

public sealed class StartSessionCommandHandler(
    IPuzzleRepository puzzleRepository,
    ISessionRepository sessionRepository,
    IAiService aiService) : IRequestHandler<StartSessionCommand, SessionDto>
{
    public async Task<SessionDto> Handle(StartSessionCommand request, CancellationToken cancellationToken)
    {
        Puzzle puzzle;

        // Try AI-generated puzzle first, fall back to seed data
        var aiResult = await aiService.GenerateHangmanPuzzleAsync(cancellationToken);

        if (aiResult is not null)
        {
            var payload = new HangmanPayload
            {
                Word = aiResult.Word,
                Description = aiResult.Description,
                Category = aiResult.Category,
                MaxAttempts = 6
            };

            puzzle = new Puzzle
            {
                Id = Guid.NewGuid(),
                GameType = request.GameType,
                Payload = JsonSerializer.Serialize(payload),
                CreatedAtUtc = DateTime.UtcNow
            };

            puzzleRepository.Add(puzzle);
        }
        else
        {
            // Fallback: pick a random seed puzzle
            var puzzles = await puzzleRepository.GetByGameTypeAsync(request.GameType, cancellationToken);

            if (puzzles.Count == 0)
                throw new InvalidOperationException($"No puzzles found for game type '{request.GameType}'.");

            puzzle = puzzles[Random.Shared.Next(puzzles.Count)];
        }

        var sessionPayload = JsonSerializer.Deserialize<HangmanPayload>(puzzle.Payload)
            ?? throw new InvalidOperationException("Invalid puzzle payload.");

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            PuzzleId = puzzle.Id,
            Status = SessionStatus.InProgress,
            GuessedLetters = string.Empty,
            AttemptsLeft = sessionPayload.MaxAttempts,
            HintsUsed = 0,
            StartedAtUtc = DateTime.UtcNow
        };

        sessionRepository.Add(session);
        await sessionRepository.SaveChangesAsync(cancellationToken);

        var maskedWord = HangmanHelper.MaskWord(sessionPayload.Word, []);

        return new SessionDto(
            session.Id,
            request.GameType,
            maskedWord,
            session.AttemptsLeft,
            [],
            session.Status.ToString());
    }
}
