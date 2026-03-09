using Exit.exe.Application.Features.Sessions.Commands;
using Exit.exe.Domain.Entities;
using Exit.exe.Repository.Data.App;

namespace Exit.exe.Application.Tests;

public class RequestHintCommandHandlerTests
{
    private const string UserId = "user-1";

    private static Guid SeedSession(
        AppDbContext db,
        int hintsUsed = 0,
        SessionStatus status = SessionStatus.InProgress)
    {
        var puzzleId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        db.Puzzles.Add(new Puzzle
        {
            Id = puzzleId,
            GameType = "hangman",
            Payload = """{"word":"KAZIMIR","description":"Ancient sect","category":"Names","maxAttempts":6}"""
        });
        db.GameSessions.Add(new GameSession
        {
            Id = sessionId,
            UserId = UserId,
            PuzzleId = puzzleId,
            Status = status,
            AttemptsLeft = 6,
            HintsUsed = hintsUsed
        });
        db.SaveChanges();
        return sessionId;
    }

    [Fact]
    public async Task Handle_FirstHint_ReturnsCategory()
    {
        using var db = TestDbContextFactory.Create();
        var sessionId = SeedSession(db);
        var handler = new RequestHintCommandHandler(db);

        var result = await handler.Handle(
            new RequestHintCommand(sessionId, UserId), CancellationToken.None);

        Assert.Contains("Names", result.Hint);
        Assert.Equal(1, result.HintsUsed);
    }

    [Fact]
    public async Task Handle_SecondHint_ReturnsDescription()
    {
        using var db = TestDbContextFactory.Create();
        var sessionId = SeedSession(db, hintsUsed: 1);
        var handler = new RequestHintCommandHandler(db);

        var result = await handler.Handle(
            new RequestHintCommand(sessionId, UserId), CancellationToken.None);

        Assert.Contains("Ancient sect", result.Hint);
        Assert.Equal(2, result.HintsUsed);
    }

    [Fact]
    public async Task Handle_ThirdHint_RevealsLetter()
    {
        using var db = TestDbContextFactory.Create();
        var sessionId = SeedSession(db, hintsUsed: 2);
        var handler = new RequestHintCommandHandler(db);

        var result = await handler.Handle(
            new RequestHintCommand(sessionId, UserId), CancellationToken.None);

        Assert.StartsWith("Try the letter:", result.Hint);
        Assert.Equal(3, result.HintsUsed);
    }

    [Fact]
    public async Task Handle_MaxHintsReached_Throws()
    {
        using var db = TestDbContextFactory.Create();
        var sessionId = SeedSession(db, hintsUsed: 3);
        var handler = new RequestHintCommandHandler(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(new RequestHintCommand(sessionId, UserId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SessionEnded_Throws()
    {
        using var db = TestDbContextFactory.Create();
        var sessionId = SeedSession(db, status: SessionStatus.Failed);
        var handler = new RequestHintCommandHandler(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(new RequestHintCommand(sessionId, UserId), CancellationToken.None));
    }
}
