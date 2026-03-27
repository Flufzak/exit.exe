namespace Exit.exe.Application.Contracts;

public interface IAiService
{
    Task<AiPuzzleResult?> GenerateHangmanPuzzleAsync(CancellationToken ct);
    Task<string?> GenerateHintAsync(string word, string category, string description, IReadOnlyCollection<string> guessedLetters, int hintNumber, CancellationToken ct);
}

public sealed record AiPuzzleResult(string Word, string Description, string Category);
