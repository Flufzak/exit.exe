using System.Text.Json;
using Exit.exe.Application.Contracts;
using Exit.exe.Application.Features.Sessions.DTOs;
using Exit.exe.Domain.Entities;
using MediatR;

namespace Exit.exe.Application.Features.Sessions.Commands;

public sealed record RequestHintCommand(Guid SessionId, string UserId) : IRequest<HintResultDto>;

public sealed class RequestHintCommandHandler(
    ISessionRepository sessionRepository,
    IAiService aiService) : IRequestHandler<RequestHintCommand, HintResultDto>
{
    private const int MaxHints = 3;

    public async Task<HintResultDto> Handle(RequestHintCommand request, CancellationToken cancellationToken)
    {
        var data = await sessionRepository.GetWithPuzzleAsync(request.SessionId, request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Session '{request.SessionId}' not found.");

        var session = data.Session;

        if (session.Status != SessionStatus.InProgress)
            throw new InvalidOperationException("This session has already ended.");

        if (session.HintsUsed >= MaxHints)
            throw new InvalidOperationException("Maximum number of hints reached.");

        var payload = JsonSerializer.Deserialize<HangmanPayload>(data.PuzzlePayload)
            ?? throw new InvalidOperationException("Invalid puzzle payload.");

        var guessedLetters = HangmanHelper.ParseGuessedLetters(session.GuessedLetters);

        // Try AI hint first, fall back to seed-based hint
        var hint = await aiService.GenerateHintAsync(
            payload.Word, payload.Category, payload.Description, guessedLetters, session.HintsUsed, cancellationToken);

        hint ??= GenerateSeedHint(payload, guessedLetters, session.HintsUsed);

        session.HintsUsed++;
        await sessionRepository.SaveChangesAsync(cancellationToken);

        return new HintResultDto(hint, session.HintsUsed);
    }

    /// <summary>
    /// Generates progressive hints from seed data (fallback strategy).
    /// Hint 1: Category, Hint 2: Description, Hint 3: Reveal an unguessed letter.
    /// </summary>
    private static string GenerateSeedHint(
        HangmanPayload payload,
        IReadOnlyCollection<string> guessedLetters,
        int hintsUsed)
    {
        return hintsUsed switch
        {
            0 => $"The category is: {payload.Category}",
            1 => $"Hint: {payload.Description}",
            2 => RevealLetterHint(payload.Word, guessedLetters),
            _ => "No more hints available."
        };
    }

    private static string RevealLetterHint(string word, IReadOnlyCollection<string> guessedLetters)
    {
        var unguessed = word
            .Select(ch => ch.ToString().ToUpperInvariant())
            .Distinct()
            .Where(ch => !guessedLetters.Contains(ch, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (unguessed.Count == 0)
            return "You've already guessed all the letters!";

        var revealedLetter = unguessed[Random.Shared.Next(unguessed.Count)];
        return $"Try the letter: {revealedLetter}";
    }
}
