using Exit.exe.Application.Contracts;

namespace Exit.exe.Application.Tests;

internal sealed class AlwaysFallbackAiService : IAiService
{
    public Task<AiPuzzleResult?> GenerateHangmanPuzzleAsync(CancellationToken ct) => Task.FromResult<AiPuzzleResult?>(null);

    public Task<string?> GenerateHintAsync(string word, string category, string description, IReadOnlyCollection<string> guessedLetters, int hintNumber, CancellationToken ct) => Task.FromResult<string?>(null);
}
