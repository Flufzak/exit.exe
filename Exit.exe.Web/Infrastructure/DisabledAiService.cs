using Exit.exe.Application.Contracts;

namespace Exit.exe.Web.Infrastructure;

public sealed class DisabledAiService : IAiService
{
    public Task<AiPuzzleResult?> GenerateHangmanPuzzleAsync(string? language, CancellationToken ct)
        => Task.FromResult<AiPuzzleResult?>(null);

    public Task<string?> GenerateHintAsync(
        string word,
        string category,
        string description,
        IReadOnlyCollection<string> guessedLetters,
        int hintNumber,
        string? language,
        CancellationToken ct)
        => Task.FromResult<string?>(null);
}