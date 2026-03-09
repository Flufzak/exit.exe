using Exit.exe.Domain.Entities;

namespace Exit.exe.Application.Contracts;

public interface ISessionRepository
{
    Task<GameSession?> GetWithPuzzleAsync(Guid sessionId, string userId, CancellationToken ct);
    void Add(GameSession session);
    Task SaveChangesAsync(CancellationToken ct);
}
