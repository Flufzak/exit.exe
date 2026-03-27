using Exit.exe.Application.Features.Profile.Queries;
using Exit.exe.Domain.Entities;
using Exit.exe.Repository.Repositories;

namespace Exit.exe.Application.Tests;

public class GetProfileStatsQueryHandlerTests
{
    private const string UserId = "profile-user-1";

    [Fact]
    public async Task Handle_NoSessions_ReturnsZeroStats()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new GetProfileStatsQueryHandler(new SessionRepository(db));

        var result = await handler.Handle(
            new GetProfileStatsQuery("no-sessions-user"), CancellationToken.None);

        Assert.Equal(0, result.TotalGamesPlayed);
        Assert.Equal(0, result.GamesWon);
        Assert.Equal(0, result.GamesLost);
        Assert.Equal(0, result.GamesInProgress);
        Assert.Equal(0, result.TotalHintsUsed);
        Assert.Null(result.BestScore);
        Assert.Null(result.AverageScore);
        Assert.Null(result.LastPlayedAtUtc);
    }

    [Fact]
    public async Task Handle_MixedSessions_ReturnsCorrectStats()
    {
        using var db = TestDbContextFactory.Create();
        var puzzleId = SeedPuzzle(db);

        db.GameSessions.AddRange(
            CreateSession(puzzleId, SessionStatus.Success, score: 80, hintsUsed: 1,
                startedAt: new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreateSession(puzzleId, SessionStatus.Success, score: 100, hintsUsed: 0,
                startedAt: new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreateSession(puzzleId, SessionStatus.Failed, score: 0, hintsUsed: 2,
                startedAt: new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreateSession(puzzleId, SessionStatus.InProgress, score: null, hintsUsed: 1,
                startedAt: new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc)));
        await db.SaveChangesAsync();

        var handler = new GetProfileStatsQueryHandler(new SessionRepository(db));

        var result = await handler.Handle(
            new GetProfileStatsQuery(UserId), CancellationToken.None);

        Assert.Equal(4, result.TotalGamesPlayed);
        Assert.Equal(2, result.GamesWon);
        Assert.Equal(1, result.GamesLost);
        Assert.Equal(1, result.GamesInProgress);
        Assert.Equal(4, result.TotalHintsUsed);
        Assert.Equal(100, result.BestScore);
        Assert.Equal(60.0, result.AverageScore); // (80 + 100 + 0) / 3
        Assert.Equal(new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc), result.LastPlayedAtUtc);
    }

    [Fact]
    public async Task Handle_OnlyCountsSessionsForGivenUser()
    {
        using var db = TestDbContextFactory.Create();
        var puzzleId = SeedPuzzle(db);

        db.GameSessions.AddRange(
            CreateSession(puzzleId, SessionStatus.Success, score: 90, userId: UserId),
            CreateSession(puzzleId, SessionStatus.Success, score: 70, userId: "other-user"));
        await db.SaveChangesAsync();

        var handler = new GetProfileStatsQueryHandler(new SessionRepository(db));

        var result = await handler.Handle(
            new GetProfileStatsQuery(UserId), CancellationToken.None);

        Assert.Equal(1, result.TotalGamesPlayed);
        Assert.Equal(1, result.GamesWon);
        Assert.Equal(90, result.BestScore);
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
        SessionStatus status = SessionStatus.InProgress,
        int? score = null,
        int hintsUsed = 0,
        DateTime? startedAt = null,
        string userId = UserId)
    {
        return new GameSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PuzzleId = puzzleId,
            Status = status,
            Score = score,
            HintsUsed = hintsUsed,
            AttemptsLeft = 6,
            StartedAtUtc = startedAt ?? DateTime.UtcNow,
            CompletedAtUtc = status != SessionStatus.InProgress ? DateTime.UtcNow : null
        };
    }
}
