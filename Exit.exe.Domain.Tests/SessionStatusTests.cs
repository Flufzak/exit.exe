using Exit.exe.Domain.Entities;

namespace Exit.exe.Domain.Tests;

public class SessionStatusTests
{
    [Fact]
    public void SessionStatus_HasExpectedValues()
    {
        Assert.Equal(0, (int)SessionStatus.InProgress);
        Assert.Equal(1, (int)SessionStatus.Success);
        Assert.Equal(2, (int)SessionStatus.Failed);
    }

    [Fact]
    public void SessionStatus_HasExactlyThreeValues()
    {
        var values = Enum.GetValues<SessionStatus>();
        Assert.Equal(3, values.Length);
    }
}
