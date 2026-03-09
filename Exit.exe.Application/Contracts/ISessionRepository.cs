using Exit.exe.Application.Features.Sessions.DTOs;
using Exit.exe.Domain.Entities;

namespace Exit.exe.Application.Contracts;

public sealed record SessionWithPuzzle(
    GameSession Session,
    string PuzzlePayload,
    string PuzzleGameType);

public interface ISessionRepository
{
    Task<SessionWithPuzzle?> GetWithPuzzleAsync(Guid sessionId, string userId, CancellationToken ct);
    Task<IReadOnlyList<SessionSummaryDto>> GetHistoryByUserAsync(string userId, int limit, int offset, CancellationToken ct);
    void Add(GameSession session);
    Task SaveChangesAsync(CancellationToken ct);
}
