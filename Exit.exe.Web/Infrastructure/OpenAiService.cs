using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Exit.exe.Application.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Exit.exe.Web.Infrastructure;

public sealed class AiProviderOptions
{
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "llama3.2";
    public string Provider { get; set; } = "ollama";
}

public sealed class OpenAiService(HttpClient httpClient, IOptions<AiProviderOptions> options, ILogger<OpenAiService> logger) : IAiService
{
    private readonly AiProviderOptions _options = options.Value;

    public async Task<AiPuzzleResult?> GenerateHangmanPuzzleAsync(string? language, CancellationToken ct)
    {
        var normalizedLanguage = NormalizeLanguage(language);
        var promptLanguage = MapPromptLanguage(normalizedLanguage);

        var prompt = $$"""
    Generate a single {{promptLanguage}} word for a Hangman game in an escape-room setting themed around a dark ancient sect called "Kazimir".
    The word should be between 5 and 10 letters, using only A-Z characters.
    Respond ONLY with valid JSON in this exact format, no extra text:
    {"word":"EXAMPLE","description":"A short clue about the word","category":"CategoryName"}
    """;

        try
        {
            var json = await SendChatRequestAsync(prompt, ct);
            if (json is null) return null;

            var result = JsonSerializer.Deserialize<AiPuzzleJson>(json);
            if (result is null || string.IsNullOrWhiteSpace(result.Word)) return null;

            var word = result.Word.Trim().ToUpperInvariant();

            if (word.Length < 3 || word.Length > 15 || !word.All(char.IsLetter))
            {
                logger.LogWarning("AI returned invalid word: {Word}", word);
                return null;
            }

            return new AiPuzzleResult(
                word,
                result.Description ?? "Mystery word",
                result.Category ?? "General",
                new AiPuzzleNarrativeResult(
                    "A dark presence watches your every move. Guess wisely.",
                    "You uncovered the truth hidden in the word. The path forward opens.",
                    "The final bell tolls. The ritual completes before you can escape."
                ),
                6);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to generate AI puzzle, falling back to seed data");
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
        var normalizedLanguage = NormalizeLanguage(language);
        var promptLanguage = MapPromptLanguage(normalizedLanguage);
        var guessed = guessedLetters.Count > 0 ? string.Join(", ", guessedLetters) : "none";

        var prompt = $"""
            You are a hint generator for a Hangman game in an escape-room with a dark ancient sect theme called "Kazimir".
            Write the hint in {promptLanguage}.
            The secret word is "{word}" (category: {category}, description: {description}).
            The player has already guessed these letters: {guessed}.
            This is hint number {hintNumber + 1} out of 3.

            Give a creative, atmospheric hint that fits the dark escape-room theme.
            Do NOT reveal the word directly. Do NOT mention any unguessed letters.
            Keep the hint to 1-2 sentences maximum.
            Respond with ONLY the hint text, no quotes or extra formatting.
            """;

        try
        {
            return await SendChatRequestAsync(prompt, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to generate AI hint, falling back to seed hint");
            return null;
        }
    }

    private async Task<string?> SendChatRequestAsync(string prompt, CancellationToken ct)
    {
        var requestBody = new
        {
            model = _options.Model,
            messages = new[]
            {
                new { role = "system", content = "You are a concise game master for a dark escape-room puzzle game." },
                new { role = "user", content = prompt }
            },
            temperature = 0.8,
            max_tokens = 200
        };

        var endpoint = $"{_options.BaseUrl.TrimEnd('/')}/v1/chat/completions";
        var response = await httpClient.PostAsJsonAsync(endpoint, requestBody, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            logger.LogWarning("{Provider} API returned {StatusCode}: {Body}", _options.Provider, response.StatusCode, errorBody);
            return null;
        }

        var completion = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: ct);
        return completion?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
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

    private static string MapPromptLanguage(string language)
    {
        return language switch
        {
            "nl" => "Dutch",
            "fr" => "French",
            "de" => "German",
            _ => "English"
        };
    }

    private sealed class AiPuzzleJson
    {
        [JsonPropertyName("word")]
        public string? Word { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }
    }

    private sealed class ChatCompletionResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    }

    private sealed class Choice
    {
        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }

    private sealed class Message
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}