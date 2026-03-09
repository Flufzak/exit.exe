using Exit.exe.Application.Contracts;
using Exit.exe.Domain.Entities;
using Exit.exe.Repository.Data.App;
using Microsoft.EntityFrameworkCore;

namespace Exit.exe.Repository.Repositories;

public sealed class SessionRepository(AppDbContext db) : ISessionRepository
{
    public async Task<GameSession?> GetWithPuzzleAsync(Guid sessionId, string userId, CancellationToken ct)
    {
        return await db.GameSessions
            .Include(s => s.Puzzle)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, ct);
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
