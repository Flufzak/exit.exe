using Exit.exe.Domain.Entities;

namespace Exit.exe.Domain.Tests;

public class PuzzleTests
{
    [Fact]
    public void NewPuzzle_SessionsCollection_IsEmpty()
    {
        var puzzle = new Puzzle
        {
            GameType = "hangman",
            Payload = "{}"
        };

        Assert.Empty(puzzle.Sessions);
    }

    [Fact]
    public void NewPuzzle_CreatedAtUtc_IsSet()
    {
        var before = DateTime.UtcNow;

        var puzzle = new Puzzle
        {
            GameType = "hangman",
            Payload = "{}"
        };

        Assert.InRange(puzzle.CreatedAtUtc, before, DateTime.UtcNow);
    }
}
