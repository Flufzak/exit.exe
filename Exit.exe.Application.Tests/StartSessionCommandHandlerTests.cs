using Exit.exe.Application.Features.Sessions.Commands;
using Exit.exe.Domain.Entities;
using Exit.exe.Repository.Repositories;

namespace Exit.exe.Application.Tests;

public class StartSessionCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidGameType_CreatesSessionAndReturnsMaskedWord()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();

        // Use a unique game type to avoid picking up seed data
        db.Puzzles.Add(new Puzzle
        {
            Id = Guid.NewGuid(),
            GameType = "hangman-test",
            Payload = """{"word":"TEST","description":"A test word","category":"Testing","maxAttempts":6}"""
        });
        await db.SaveChangesAsync();

        var handler = new StartSessionCommandHandler(
            new PuzzleRepository(db), new SessionRepository(db));
        var command = new StartSessionCommand("hangman-test", "user-1");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("hangman-test", result.GameType);
        Assert.Equal("_ _ _ _", result.MaskedWord);
        Assert.Equal(6, result.AttemptsLeft);
        Assert.Empty(result.GuessedLetters);
        Assert.Equal("InProgress", result.Status);
    }

    [Fact]
    public async Task Handle_UnknownGameType_Throws()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new StartSessionCommandHandler(
            new PuzzleRepository(db), new SessionRepository(db));
        var command = new StartSessionCommand("unknown", "user-1");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));
    }
}
