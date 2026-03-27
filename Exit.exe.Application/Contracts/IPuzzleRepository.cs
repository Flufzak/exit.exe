using Exit.exe.Domain.Entities;

namespace Exit.exe.Application.Contracts;

public interface IPuzzleRepository
{
    Task<IReadOnlyList<Puzzle>> GetByGameTypeAsync(string gameType, CancellationToken ct);
    void Add(Puzzle puzzle);
}
