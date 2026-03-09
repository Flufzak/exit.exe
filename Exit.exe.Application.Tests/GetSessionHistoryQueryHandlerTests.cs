using Exit.exe.Application.Features.Sessions.Queries;
using Exit.exe.Domain.Entities;
using Exit.exe.Repository.Repositories;

namespace Exit.exe.Application.Tests;

public class GetSessionHistoryQueryHandlerTests
{
    private const string UserId = "user-1";

    [Fact]
    public async Task Handle_ReturnsSessionsOrderedByStartedAtDescending()
    {
        using var db = TestDbContextFactory.Create();
        var puzzleId = SeedPuzzle(db);

        db.GameSessions.AddRange(
            CreateSession(puzzleId, new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreateSession(puzzleId, new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreateSession(puzzleId, new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc)));
        await db.SaveChangesAsync();

        var handler = new GetSessionHistoryQueryHandler(new SessionRepository(db));

        var result = await handler.Handle(
            new GetSessionHistoryQuery(UserId), CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.True(result[0].StartedAtUtc > result[1].StartedAtUtc);
        Assert.True(result[1].StartedAtUtc > result[2].StartedAtUtc);
    }

    [Fact]
    public async Task Handle_NoSessions_ReturnsEmptyList()
    {
        using var db = TestDbContextFactory.Create();

        var handler = new GetSessionHistoryQueryHandler(new SessionRepository(db));

        var result = await handler.Handle(
            new GetSessionHistoryQuery("no-sessions-user"), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_OnlyReturnsSessionsForGivenUser()
    {
        using var db = TestDbContextFactory.Create();
        var puzzleId = SeedPuzzle(db);

        db.GameSessions.AddRange(
            CreateSession(puzzleId, userId: UserId),
            CreateSession(puzzleId, userId: UserId),
            CreateSession(puzzleId, userId: "other-user"));
        await db.SaveChangesAsync();

        var handler = new GetSessionHistoryQueryHandler(new SessionRepository(db));

        var result = await handler.Handle(
            new GetSessionHistoryQuery(UserId), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_RespectsLimitAndOffset()
    {
        using var db = TestDbContextFactory.Create();
        var puzzleId = SeedPuzzle(db);

        for (var i = 0; i < 5; i++)
            db.GameSessions.Add(CreateSession(puzzleId,
                new DateTime(2025, 1 + i, 1, 0, 0, 0, DateTimeKind.Utc)));
        await db.SaveChangesAsync();

        var handler = new GetSessionHistoryQueryHandler(new SessionRepository(db));

        var result = await handler.Handle(
            new GetSessionHistoryQuery(UserId, Limit: 2, Offset: 1), CancellationToken.None);

        Assert.Equal(2, result.Count);
        // Ordered desc: May, Apr, Mar, Feb, Jan → skip 1 (May) → take 2 (Apr, Mar)
        Assert.Equal(new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc), result[0].StartedAtUtc);
        Assert.Equal(new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc), result[1].StartedAtUtc);
    }

    private static Guid SeedPuzzle(Repository.Data.App.AppDbContext db)
    {
        var puzzleId = Guid.NewGuid();
        db.Puzzles.Add(new Puzzle
        {
            Id = puzzleId,
            GameType = "hangman",
            Payload = """{"word":"TEST","description":"desc","category":"cat","maxAttempts":6}"""
        });
        db.SaveChanges();
        return puzzleId;
    }

    private static GameSession CreateSession(
        Guid puzzleId,
        DateTime? startedAt = null,
        string userId = UserId)
    {
        return new GameSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PuzzleId = puzzleId,
            Status = SessionStatus.InProgress,
            AttemptsLeft = 6,
            StartedAtUtc = startedAt ?? DateTime.UtcNow
        };
    }
}
