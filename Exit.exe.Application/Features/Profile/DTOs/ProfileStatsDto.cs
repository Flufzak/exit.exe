namespace Exit.exe.Application.Features.Profile.DTOs;

public sealed record ProfileStatsDto(
    int TotalGamesPlayed,
    int GamesWon,
    int GamesLost,
    int GamesInProgress,
    int TotalHintsUsed,
    int? BestScore,
    double? AverageScore,
    DateTime? LastPlayedAtUtc);
