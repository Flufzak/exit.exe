using Exit.exe.Application.Features.Sessions.Commands;
using Exit.exe.Domain.Entities;
using Exit.exe.Repository.Data.App;
using Exit.exe.Repository.Repositories;

namespace Exit.exe.Application.Tests;

public class SubmitGuessCommandHandlerTests
{
    private const string UserId = "user-1";

    private static (Guid sessionId, Guid puzzleId) SeedSession(
        AppDbContext db,
        string word = "TEST",
        int attemptsLeft = 6,
        string guessedLetters = "",
        SessionStatus status = SessionStatus.InProgress)
    {
        var puzzleId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        db.Puzzles.Add(new Puzzle
        {
            Id = puzzleId,
            GameType = "hangman",
            Payload = $$$"""{"word":"{{{word}}}","description":"desc","category":"cat","maxAttempts":6}"""
        });
        db.GameSessions.Add(new GameSession
        {
            Id = sessionId,
            UserId = UserId,
            PuzzleId = puzzleId,
            Status = status,
            AttemptsLeft = attemptsLeft,
            GuessedLetters = guessedLetters
        });
        db.SaveChanges();
        return (sessionId, puzzleId);
    }

    [Fact]
    public async Task Handle_CorrectLetter_RevealsLetterAndKeepsAttempts()
    {
        using var db = TestDbContextFactory.Create();
        var (sessionId, _) = SeedSession(db);
        var handler = new SubmitGuessCommandHandler(new SessionRepository(db));

        var result = await handler.Handle(
            new SubmitGuessCommand(sessionId, "T", UserId), CancellationToken.None);

        Assert.True(result.Correct);
        Assert.Equal("T _ _ T", result.MaskedWord);
        Assert.Equal(6, result.AttemptsLeft);
        Assert.Equal("InProgress", result.Status);
    }

    [Fact]
    public async Task Handle_WrongLetter_DecrementsAttempts()
    {
        using var db = TestDbContextFactory.Create();
        var (sessionId, _) = SeedSession(db);
        var handler = new SubmitGuessCommandHandler(new SessionRepository(db));

        var result = await handler.Handle(
            new SubmitGuessCommand(sessionId, "X", UserId), CancellationToken.None);

        Assert.False(result.Correct);
        Assert.Equal(5, result.AttemptsLeft);
        Assert.Equal("InProgress", result.Status);
    }

    [Fact]
    public async Task Handle_AllLettersGuessed_StatusSuccess()
    {
        using var db = TestDbContextFactory.Create();
        var (sessionId, _) = SeedSession(db, word: "AB", guessedLetters: "A");
        var handler = new SubmitGuessCommandHandler(new SessionRepository(db));

        var result = await handler.Handle(
            new SubmitGuessCommand(sessionId, "B", UserId), CancellationToken.None);

        Assert.True(result.Correct);
        Assert.Equal("A B", result.MaskedWord);
        Assert.Equal("Success", result.Status);
    }

    [Fact]
    public async Task Handle_LastAttemptWrong_StatusFailed()
    {
        using var db = TestDbContextFactory.Create();
        var (sessionId, _) = SeedSession(db, attemptsLeft: 1);
        var handler = new SubmitGuessCommandHandler(new SessionRepository(db));

        var result = await handler.Handle(
            new SubmitGuessCommand(sessionId, "X", UserId), CancellationToken.None);

        Assert.False(result.Correct);
        Assert.Equal(0, result.AttemptsLeft);
        Assert.Equal("Failed", result.Status);
    }

    [Fact]
    public async Task Handle_DuplicateLetter_Throws()
    {
        using var db = TestDbContextFactory.Create();
        var (sessionId, _) = SeedSession(db, guessedLetters: "T");
        var handler = new SubmitGuessCommandHandler(new SessionRepository(db));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(new SubmitGuessCommand(sessionId, "T", UserId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SessionAlreadyEnded_Throws()
    {
        using var db = TestDbContextFactory.Create();
        var (sessionId, _) = SeedSession(db, status: SessionStatus.Success);
        var handler = new SubmitGuessCommandHandler(new SessionRepository(db));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(new SubmitGuessCommand(sessionId, "A", UserId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvalidLetter_Throws()
    {
        using var db = TestDbContextFactory.Create();
        var (sessionId, _) = SeedSession(db);
        var handler = new SubmitGuessCommandHandler(new SessionRepository(db));

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(new SubmitGuessCommand(sessionId, "12", UserId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WrongUserId_Throws()
    {
        using var db = TestDbContextFactory.Create();
        var (sessionId, _) = SeedSession(db);
        var handler = new SubmitGuessCommandHandler(new SessionRepository(db));

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new SubmitGuessCommand(sessionId, "A", "other-user"), CancellationToken.None));
    }
}
