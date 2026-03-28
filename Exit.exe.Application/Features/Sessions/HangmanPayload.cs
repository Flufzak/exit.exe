using System.Text.Json.Serialization;

namespace Exit.exe.Application.Features.Sessions;

public sealed class HangmanPayload
{
    [JsonPropertyName("word")]
    public string Word { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("maxAttempts")]
    public int MaxAttempts { get; set; } = 6;

    [JsonPropertyName("narrative")]
    public HangmanNarrative Narrative { get; set; } = new();
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