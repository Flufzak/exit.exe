using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Exit.exe.Application.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Exit.exe.Web.Infrastructure;

public sealed class AiCoreOptions
{
    public string BaseUrl { get; set; } = "http://localhost:8000";
}

public sealed class AiCoreService(
    HttpClient httpClient,
    IOptions<AiCoreOptions> options,
    ILogger<AiCoreService> logger) : IAiService
{
    private readonly AiCoreOptions _options = options.Value;

    public async Task<AiPuzzleResult?> GenerateHangmanPuzzleAsync(string? language, CancellationToken ct)
    {
        try
        {
            var normalizedLanguage = NormalizeLanguage(language);
            var endpoint = $"{_options.BaseUrl.TrimEnd('/')}/puzzles/generate";

            logger.LogInformation("Generating hangman puzzle in language: {Language}", normalizedLanguage);

            var request = new GeneratePuzzleRequest
            {
                Room = 1,
                Type = "hangman",
                Difficulty = 4,
                ThemeWord = null,
                Language = normalizedLanguage
            };

            var response = await httpClient.PostAsJsonAsync(endpoint, request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                logger.LogWarning(
                    "AI core puzzle generation returned {StatusCode}: {Body}",
                    response.StatusCode,
                    errorBody);
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<GeneratePuzzleResponse>(cancellationToken: ct);

            var word =
                payload?.Mechanics?.TargetWord?.Trim().ToUpperInvariant()
                ?? payload?.Solution?.Word?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(word))
            {
                logger.LogWarning("AI core puzzle generation returned no valid target word.");
                return null;
            }

            if (word.Length < 3 || word.Length > 16 || !word.All(char.IsLetter))
            {
                logger.LogWarning("AI core puzzle generation returned invalid word: {Word}", word);
                return null;
            }

            var description =
                payload?.Description?.Trim()
                ?? payload?.Narrative?.Intro?.Trim()
                ?? "A hidden truth awaits.";

            var category =
                payload?.Category?.Trim()
                ?? "General";

            var intro =
                payload?.Narrative?.Intro?.Trim()
                ?? "A dark presence watches your every move. Guess wisely.";

            var success =
                payload?.Narrative?.Success?.Trim()
                ?? "You uncovered the truth hidden in the word. The path forward opens.";

            var failure =
                payload?.Narrative?.Failure?.Trim()
                ?? "The final bell tolls. The ritual completes before you can escape.";

            var maxAttempts = payload?.Mechanics?.MaxAttempts ?? 6;

            return new AiPuzzleResult(
                word,
                description,
                category,
                new AiPuzzleNarrativeResult(intro, success, failure),
                maxAttempts);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "AI core puzzle generation failed.");
            return null;
        }
    }

    public async Task<string?> GenerateHintAsync(
        string word,
        string category,
        string description,
        IReadOnlyCollection<string> guessedLetters,
        int hintNumber,
        string? language,
        CancellationToken ct)
    {
        try
        {
            var normalizedLanguage = NormalizeLanguage(language);
            var endpoint = $"{_options.BaseUrl.TrimEnd('/')}/hangman/hint";

            logger.LogInformation("Generating hangman hint in language: {Language}", normalizedLanguage);

            var request = new HangmanHintRequest
            {
                Word = word,
                Category = category,
                Description = description,
                GuessedLetters = guessedLetters
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim().ToUpperInvariant())
                    .ToList(),
                HintNumber = Math.Clamp(hintNumber, 0, 2),
                AttemptsLeft = 6,
                Language = normalizedLanguage
            };

            var response = await httpClient.PostAsJsonAsync(endpoint, request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                logger.LogWarning(
                    "AI core hint generation returned {StatusCode}: {Body}",
                    response.StatusCode,
                    errorBody);
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<HangmanHintResponse>(cancellationToken: ct);
            var hint = payload?.Hint?.Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(hint))
            {
                logger.LogWarning("AI core hint generation returned an empty hint.");
                return null;
            }

            return hint;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "AI core hint generation failed.");
            return null;
        }
    }

    private static string NormalizeLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return "en";
        }

        var lang = language.Trim().ToLowerInvariant();

        if (lang.StartsWith("nl")) return "nl";
        if (lang.StartsWith("fr")) return "fr";
        if (lang.StartsWith("de")) return "de";
        if (lang.StartsWith("en")) return "en";

        return "en";
    }

    private sealed class GeneratePuzzleRequest
    {
        [JsonPropertyName("room")]
        public int Room { get; set; }

        [JsonPropertyName("type")]
        public required string Type { get; set; }

        [JsonPropertyName("difficulty")]
        public int Difficulty { get; set; }

        [JsonPropertyName("theme_word")]
        public string? ThemeWord { get; set; }

        [JsonPropertyName("language")]
        public required string Language { get; set; }
    }

    private sealed class GeneratePuzzleResponse
    {
        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("mechanics")]
        public MechanicsPayload? Mechanics { get; set; }

        [JsonPropertyName("narrative")]
        public NarrativePayload? Narrative { get; set; }

        [JsonPropertyName("solution")]
        public SolutionPayload? Solution { get; set; }
    }

    private sealed class MechanicsPayload
    {
        [JsonPropertyName("target_word")]
        public string? TargetWord { get; set; }

        [JsonPropertyName("max_attempts")]
        public int? MaxAttempts { get; set; }
    }

    private sealed class NarrativePayload
    {
        [JsonPropertyName("intro")]
        public string? Intro { get; set; }

        [JsonPropertyName("success")]
        public string? Success { get; set; }

        [JsonPropertyName("failure")]
        public string? Failure { get; set; }
    }

    private sealed class SolutionPayload
    {
        [JsonPropertyName("word")]
        public string? Word { get; set; }
    }

    private sealed class HangmanHintRequest
    {
        [JsonPropertyName("word")]
        public required string Word { get; set; }

        [JsonPropertyName("category")]
        public required string Category { get; set; }

        [JsonPropertyName("description")]
        public required string Description { get; set; }

        [JsonPropertyName("guessed_letters")]
        public required List<string> GuessedLetters { get; set; }

        [JsonPropertyName("hint_number")]
        public int HintNumber { get; set; }

        [JsonPropertyName("attempts_left")]
        public int AttemptsLeft { get; set; }

        [JsonPropertyName("language")]
        public required string Language { get; set; }
    }

    private sealed class HangmanHintResponse
    {
        [JsonPropertyName("hint")]
        public string? Hint { get; set; }
    }
}