namespace Exit.exe.Domain.Entities;

public sealed class GameSession
{
    public Guid Id { get; set; }

    /// <summary>Foreign key to the ASP.NET Identity user (lives in AuthDbContext, so no navigation property).</summary>
    public required string UserId { get; set; }

    public Guid PuzzleId { get; set; }

    public SessionStatus Status { get; set; } = SessionStatus.InProgress;

    /// <summary>Comma-separated uppercase letters that have been guessed, e.g. "A,E,K".</summary>
    public string GuessedLetters { get; set; } = string.Empty;

    public int AttemptsLeft { get; set; } = 6;

    public int HintsUsed { get; set; }

    public int? Score { get; set; }

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAtUtc { get; set; }

    // Navigation
    public Puzzle Puzzle { get; set; } = null!;
}
