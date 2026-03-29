using System.Text.Json.Serialization;

namespace Exit.exe.Application.Features.Sessions;

public sealed class HangmanPayload
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("narrative")]
    public HangmanNarrative Narrative { get; set; } = new();

    [JsonPropertyName("mechanics")]
    public HangmanMechanics Mechanics { get; set; } = new();

    [JsonPropertyName("solution")]
    public HangmanSolution Solution { get; set; } = new();
}

public sealed class HangmanNarrative
{
    [JsonPropertyName("intro")]
    public string Intro { get; set; } = string.Empty;

    [JsonPropertyName("success")]
    public string Success { get; set; } = string.Empty;

    [JsonPropertyName("failure")]
    public string Failure { get; set; } = string.Empty;
}

public sealed class HangmanMechanics
{
    private string _targetWord = string.Empty;
    private int _maxAttempts = 6;

    [JsonPropertyName("targetWord")]
    public string TargetWord
    {
        get => _targetWord;
        set => _targetWord = value ?? string.Empty;
    }

    [JsonPropertyName("maxAttempts")]
    public int MaxAttempts
    {
        get => _maxAttempts;
        set => _maxAttempts = value;
    }

    [JsonPropertyName("target_word")]
    public string TargetWordSnake
    {
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _targetWord = value;
            }
        }
    }

    [JsonPropertyName("max_attempts")]
    public int MaxAttemptsSnake
    {
        set => _maxAttempts = value;
    }
}

public sealed class HangmanSolution
{
    [JsonPropertyName("word")]
    public string Word { get; set; } = string.Empty;
}