namespace Exit.exe.Application.Contracts;

public interface IAiService
{
    Task<AiPuzzleResult?> GenerateHangmanPuzzleAsync(string? language, CancellationToken ct);

    Task<string?> GenerateHintAsync(
        string word,
        string category,
        string description,
        IReadOnlyCollection<string> guessedLetters,
        int hintNumber,
        string? language,
        CancellationToken ct);
}

public sealed record AiPuzzleResult(
    string Word,
    string Description,
    string Category,
    AiPuzzleNarrativeResult Narrative,
    int MaxAttempts);

public sealed record AiPuzzleNarrativeResult(
    string Intro,
    string Success,
    string Failure);