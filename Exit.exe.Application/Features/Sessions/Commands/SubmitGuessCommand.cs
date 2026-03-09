using System.Text.Json;
using Exit.exe.Application.Contracts;
using Exit.exe.Application.Features.Sessions.DTOs;
using Exit.exe.Domain.Entities;
using MediatR;

namespace Exit.exe.Application.Features.Sessions.Commands;

public sealed record SubmitGuessCommand(Guid SessionId, string Letter, string UserId) : IRequest<GuessResultDto>;

public sealed class SubmitGuessCommandHandler(ISessionRepository sessionRepository) : IRequestHandler<SubmitGuessCommand, GuessResultDto>
{
    public async Task<GuessResultDto> Handle(SubmitGuessCommand request, CancellationToken cancellationToken)
    {
        var session = await sessionRepository.GetWithPuzzleAsync(request.SessionId, request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Session '{request.SessionId}' not found.");

        if (session.Status != SessionStatus.InProgress)
            throw new InvalidOperationException("This session has already ended.");

        var payload = JsonSerializer.Deserialize<HangmanPayload>(session.Puzzle.Payload)
            ?? throw new InvalidOperationException("Invalid puzzle payload.");

        var letter = request.Letter.ToUpperInvariant();

        if (letter.Length != 1 || !char.IsLetter(letter[0]))
            throw new ArgumentException("Guess must be a single letter.");

        var guessedLetters = HangmanHelper.ParseGuessedLetters(session.GuessedLetters);

        // Already guessed this letter
        if (guessedLetters.Contains(letter, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Letter '{letter}' has already been guessed.");

        guessedLetters.Add(letter);

        var word = payload.Word.ToUpperInvariant();
        var correct = word.Contains(letter, StringComparison.OrdinalIgnoreCase);

        if (!correct)
        {
            session.AttemptsLeft--;
        }

        session.GuessedLetters = string.Join(",", guessedLetters);

        // Check win/loss
        if (HangmanHelper.IsWordFullyGuessed(word, guessedLetters))
        {
            session.Status = SessionStatus.Success;
            session.CompletedAtUtc = DateTime.UtcNow;
            session.Score = CalculateScore(session.AttemptsLeft, session.HintsUsed, payload.MaxAttempts);
        }
        else if (session.AttemptsLeft <= 0)
        {
            session.Status = SessionStatus.Failed;
            session.CompletedAtUtc = DateTime.UtcNow;
            session.Score = 0;
        }

        await sessionRepository.SaveChangesAsync(cancellationToken);

        var maskedWord = HangmanHelper.MaskWord(word, guessedLetters);

        return new GuessResultDto(
            correct,
            maskedWord,
            session.AttemptsLeft,
            guessedLetters,
            session.Status.ToString());
    }

    private static int CalculateScore(int attemptsLeft, int hintsUsed, int maxAttempts)
    {
        // Base score: 100, minus penalty for wrong guesses and hints
        var wrongGuesses = maxAttempts - attemptsLeft;
        var score = Math.Max(0, 100 - (wrongGuesses * 10) - (hintsUsed * 15));
        return score;
    }
}
