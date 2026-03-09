namespace Exit.exe.Application.Features.Sessions;

/// <summary>
/// Shared helpers for Hangman game logic.
/// </summary>
public static class HangmanHelper
{
    /// <summary>
    /// Returns the masked representation of the word, revealing only guessed letters.
    /// Example: word = "KAZIMIR", guessed = { "A", "I" } → "_ A _ I _ I _"
    /// </summary>
    public static string MaskWord(string word, IReadOnlyCollection<string> guessedLetters)
    {
        var masked = word.Select(ch =>
            guessedLetters.Contains(ch.ToString(), StringComparer.OrdinalIgnoreCase)
                ? ch.ToString()
                : "_");

        return string.Join(" ", masked);
    }

    /// <summary>
    /// Parses comma-separated guessed letters into a list.
    /// </summary>
    public static List<string> ParseGuessedLetters(string guessedLetters)
    {
        if (string.IsNullOrWhiteSpace(guessedLetters))
            return [];

        return guessedLetters
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    /// <summary>
    /// Checks if all letters of the word have been guessed.
    /// </summary>
    public static bool IsWordFullyGuessed(string word, IReadOnlyCollection<string> guessedLetters)
    {
        return word.All(ch =>
            guessedLetters.Contains(ch.ToString(), StringComparer.OrdinalIgnoreCase));
    }
}
