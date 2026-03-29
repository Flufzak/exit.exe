using System.Text.Json;
using Exit.exe.Application.Features.Sessions.Commands;
using Exit.exe.Domain.Entities;
using Exit.exe.Repository.Repositories;

namespace Exit.exe.Application.Tests;

public class StartSessionCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidGameType_CreatesSessionAndReturnsMaskedWord()
    {
        using var db = TestDbContextFactory.Create();

        var payload = JsonSerializer.Serialize(new
        {
            category = "Testing",
            description = "A test word",
            narrative = new
            {
                intro = "Intro",
                success = "Success",
                failure = "Failure"
            },
            mechanics = new
            {
                targetWord = "TEST",
                maxAttempts = 6
            },
            solution = new
            {
                word = "TEST"
            }
        });

        db.Puzzles.Add(new Puzzle
        {
            Id = Guid.NewGuid(),
            GameType = "story-1",
            Payload = payload
        });

        await db.SaveChangesAsync();

        var handler = new StartSessionCommandHandler(
            new PuzzleRepository(db),
            new SessionRepository(db),
            new AlwaysFallbackAiService());

        var command = new StartSessionCommand("story-1", "user-1", "nl");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("story-1", result.GameType);
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
            new PuzzleRepository(db),
            new SessionRepository(db),
            new AlwaysFallbackAiService());

        var command = new StartSessionCommand("unknown", "user-1", "nl");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));
    }
}