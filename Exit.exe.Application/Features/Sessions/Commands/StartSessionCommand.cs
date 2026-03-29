using System.Text.Json;
using Exit.exe.Application.Contracts;
using Exit.exe.Application.Features.Sessions.DTOs;
using Exit.exe.Domain.Entities;
using MediatR;

namespace Exit.exe.Application.Features.Sessions.Commands;

public sealed record StartSessionCommand(string GameType, string UserId, string? Language) : IRequest<SessionDto>;

public sealed class StartSessionCommandHandler(
    IPuzzleRepository puzzleRepository,
    ISessionRepository sessionRepository,
    IAiService aiService) : IRequestHandler<StartSessionCommand, SessionDto>
{
    private const int DefaultMaxAttempts = 6;

    public async Task<SessionDto> Handle(StartSessionCommand request, CancellationToken cancellationToken)
    {
        Puzzle puzzle;

        var aiResult = await aiService.GenerateHangmanPuzzleAsync(request.Language, cancellationToken);

        if (aiResult is not null)
        {
            var payload = new HangmanPayload
            {
                Description = aiResult.Description,
                Category = aiResult.Category,
                Narrative = new HangmanNarrative
                {
                    Intro = aiResult.Narrative.Intro,
                    Success = aiResult.Narrative.Success,
                    Failure = aiResult.Narrative.Failure
                },
                Mechanics = new HangmanMechanics
                {
                    TargetWord = aiResult.Word,
                    MaxAttempts = DefaultMaxAttempts
                },
                Solution = new HangmanSolution
                {
                    Word = aiResult.Word
                }
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
            var puzzles = await puzzleRepository.GetByGameTypeAsync(request.GameType, cancellationToken);

            if (puzzles.Count == 0)
                throw new InvalidOperationException($"No puzzles found for game type '{request.GameType}'.");

            puzzle = puzzles[Random.Shared.Next(puzzles.Count)];
        }

        var sessionPayload = JsonSerializer.Deserialize<HangmanPayload>(puzzle.Payload)
            ?? throw new InvalidOperationException("Invalid puzzle payload.");

        sessionPayload.Mechanics.MaxAttempts = DefaultMaxAttempts;

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            PuzzleId = puzzle.Id,
            Status = SessionStatus.InProgress,
            GuessedLetters = string.Empty,
            AttemptsLeft = DefaultMaxAttempts,
            HintsUsed = 0,
            StartedAtUtc = DateTime.UtcNow
        };

        sessionRepository.Add(session);
        await sessionRepository.SaveChangesAsync(cancellationToken);

        var maskedWord = HangmanHelper.MaskWord(sessionPayload.Mechanics.TargetWord, []);

        return new SessionDto(
            session.Id,
            request.GameType,
            maskedWord,
            session.AttemptsLeft,
            [],
            session.Status.ToString(),
            session.HintsUsed,
            session.Score,
            new SessionNarrativeDto(
                sessionPayload.Narrative.Intro,
                sessionPayload.Narrative.Success,
                sessionPayload.Narrative.Failure
            ));
    }
}