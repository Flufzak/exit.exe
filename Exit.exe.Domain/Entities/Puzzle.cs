namespace Exit.exe.Domain.Entities;

public sealed class Puzzle
{
    public Guid Id { get; set; }

    /// <summary>Game type code, e.g. "hangman".</summary>
    public required string GameType { get; set; }

    /// <summary>JSON payload whose shape depends on <see cref="GameType"/>.</summary>
    public required string Payload { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<GameSession> Sessions { get; set; } = [];
}
