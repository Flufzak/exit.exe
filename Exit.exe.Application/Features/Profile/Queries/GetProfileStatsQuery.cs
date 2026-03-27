using Exit.exe.Application.Contracts;
using Exit.exe.Application.Features.Profile.DTOs;
using Exit.exe.Domain.Entities;
using MediatR;

namespace Exit.exe.Application.Features.Profile.Queries;

public sealed record GetProfileStatsQuery(string UserId) : IRequest<ProfileStatsDto>;

public sealed class GetProfileStatsQueryHandler(
    ISessionRepository sessionRepository) : IRequestHandler<GetProfileStatsQuery, ProfileStatsDto>
{
    public async Task<ProfileStatsDto> Handle(GetProfileStatsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await sessionRepository.GetAllByUserAsync(request.UserId, cancellationToken);

        var totalGames = sessions.Count;
        var won = sessions.Count(s => s.Status == SessionStatus.Success);
        var lost = sessions.Count(s => s.Status == SessionStatus.Failed);
        var inProgress = sessions.Count(s => s.Status == SessionStatus.InProgress);
        var totalHints = sessions.Sum(s => s.HintsUsed);

        var completedWithScore = sessions
            .Where(s => s.Score.HasValue)
            .Select(s => s.Score!.Value)
            .ToList();

        int? bestScore = completedWithScore.Count > 0 ? completedWithScore.Max() : null;
        double? avgScore = completedWithScore.Count > 0 ? completedWithScore.Average() : null;

        var lastPlayed = sessions.Count > 0
            ? sessions.Max(s => s.StartedAtUtc)
            : (DateTime?)null;

        return new ProfileStatsDto(
            totalGames, won, lost, inProgress,
            totalHints, bestScore, avgScore, lastPlayed);
    }
}
