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
}
