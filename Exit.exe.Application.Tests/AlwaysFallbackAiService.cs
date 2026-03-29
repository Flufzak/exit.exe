using Exit.exe.Application.Contracts;

namespace Exit.exe.Application.Tests;

internal sealed class AlwaysFallbackAiService : IAiService
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
    => Task.FromResult<string?>(string.Empty);
}