using Exit.exe.Domain.Entities;

namespace Exit.exe.Domain.Tests;

public class GameSessionTests
{
    [Fact]
    public void NewGameSession_HasCorrectDefaults()
    {
        var session = new GameSession
        {
            UserId = "user-1"
        };

        Assert.Equal(SessionStatus.InProgress, session.Status);
        Assert.Equal(string.Empty, session.GuessedLetters);
        Assert.Equal(6, session.AttemptsLeft);
        Assert.Equal(0, session.HintsUsed);
        Assert.Null(session.Score);
        Assert.Null(session.CompletedAtUtc);
    }
}
