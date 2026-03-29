namespace Exit.exe.Application.Features.Sessions.DTOs;

public sealed record SessionNarrativeDto(
    string Intro,
    string Success,
    string Failure);

public sealed record SessionDto(
    Guid SessionId,
    string GameType,
    string MaskedWord,
    int AttemptsLeft,
    IReadOnlyList<string> GuessedLetters,
    string Status,
    int HintsUsed,
    int? Score,
    SessionNarrativeDto? Narrative);