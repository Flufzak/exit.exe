using Exit.exe.Application.Contracts;
using Exit.exe.Domain.Entities;
using Exit.exe.Repository.Data.App;
using Microsoft.EntityFrameworkCore;

namespace Exit.exe.Repository.Repositories;

public sealed class PuzzleRepository(AppDbContext db) : IPuzzleRepository
{
    public async Task<IReadOnlyList<Puzzle>> GetByGameTypeAsync(string gameType, CancellationToken ct)
    {
        return await db.Puzzles
            .Where(p => p.GameType == gameType)
            .ToListAsync(ct);
    }

    public void Add(Puzzle puzzle)
    {
        db.Puzzles.Add(puzzle);
    }
}
