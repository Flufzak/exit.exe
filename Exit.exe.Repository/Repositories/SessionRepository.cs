using Exit.exe.Application.Contracts;
using Exit.exe.Application.Features.Sessions.DTOs;
using Exit.exe.Domain.Entities;
using Exit.exe.Repository.Data.App;
using Microsoft.EntityFrameworkCore;

namespace Exit.exe.Repository.Repositories;

public sealed class SessionRepository(AppDbContext db) : ISessionRepository
{
    public async Task<SessionWithPuzzle?> GetWithPuzzleAsync(Guid sessionId, string userId, CancellationToken ct)
    {
        return await db.GameSessions
            .Where(s => s.Id == sessionId && s.UserId == userId)
            .Select(s => new SessionWithPuzzle(s, s.Puzzle.Payload, s.Puzzle.GameType))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<SessionSummaryDto>> GetHistoryByUserAsync(string userId, CancellationToken ct)
    {
        return await db.GameSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAtUtc)
            .Select(s => new SessionSummaryDto(
                s.Id,
                s.Puzzle.GameType,
                s.Status.ToString(),
                s.Score,
                s.HintsUsed,
                s.StartedAtUtc,
                s.CompletedAtUtc))
            .ToListAsync(ct);
    }

    public void Add(GameSession session)
    {
        db.GameSessions.Add(session);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
}
