namespace Exit.exe.Application.Features.Sessions.DTOs;

public sealed record SessionSummaryDto(
    Guid SessionId,
    string GameType,
    string Status,
    int? Score,
    int HintsUsed,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc);
