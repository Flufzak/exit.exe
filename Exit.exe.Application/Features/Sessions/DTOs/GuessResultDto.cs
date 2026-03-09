namespace Exit.exe.Application.Features.Sessions.DTOs;

public sealed record GuessResultDto(
    bool Correct,
    string MaskedWord,
    int AttemptsLeft,
    IReadOnlyList<string> GuessedLetters,
    string Status);
