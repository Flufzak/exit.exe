using Exit.exe.Application.Features.Sessions.Commands;
using Exit.exe.Application.Features.Sessions.Queries;
using Exit.exe.Application.Features.Sessions.Validators;

namespace Exit.exe.Application.Tests;

public class ValidatorTests
{
    [Fact]
    public async Task StartSession_EmptyGameType_Fails()
    {
        var validator = new StartSessionCommandValidator();
        var command = new StartSessionCommand("", "user-1", "nl");

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "GameType");
    }

    [Fact]
    public async Task StartSession_ValidCommand_Passes()
    {
        var validator = new StartSessionCommandValidator();
        var command = new StartSessionCommand("story-1", "user-1", "nl");

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task SubmitGuess_EmptyLetter_Fails()
    {
        var validator = new SubmitGuessCommandValidator();
        var command = new SubmitGuessCommand(Guid.NewGuid(), "", "user-1");

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Letter");
    }

    [Fact]
    public async Task SubmitGuess_MultipleChars_Fails()
    {
        var validator = new SubmitGuessCommandValidator();
        var command = new SubmitGuessCommand(Guid.NewGuid(), "AB", "user-1");

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task SubmitGuess_NonLetter_Fails()
    {
        var validator = new SubmitGuessCommandValidator();
        var command = new SubmitGuessCommand(Guid.NewGuid(), "5", "user-1");

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task SubmitGuess_ValidLetter_Passes()
    {
        var validator = new SubmitGuessCommandValidator();
        var command = new SubmitGuessCommand(Guid.NewGuid(), "A", "user-1");

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task RequestHint_EmptySessionId_Fails()
    {
        var validator = new RequestHintCommandValidator();
        var command = new RequestHintCommand(Guid.Empty, "user-1");

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "SessionId");
    }

    [Fact]
    public async Task RequestHint_ValidCommand_Passes()
    {
        var validator = new RequestHintCommandValidator();
        var command = new RequestHintCommand(Guid.NewGuid(), "user-1");

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task GetSessionHistory_ValidQuery_Passes()
    {
        var validator = new GetSessionHistoryQueryValidator();
        var query = new GetSessionHistoryQuery("user-1", Limit: 50, Offset: 0);

        var result = await validator.ValidateAsync(query);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetSessionHistory_LimitLessThanOrEqualToZero_Fails(int limit)
    {
        var validator = new GetSessionHistoryQueryValidator();
        var query = new GetSessionHistoryQuery("user-1", Limit: limit, Offset: 0);

        var result = await validator.ValidateAsync(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Limit");
    }

    [Fact]
    public async Task GetSessionHistory_LimitExceedsMax_Fails()
    {
        var validator = new GetSessionHistoryQueryValidator();
        var query = new GetSessionHistoryQuery("user-1", Limit: 101, Offset: 0);

        var result = await validator.ValidateAsync(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Limit");
    }

    [Fact]
    public async Task GetSessionHistory_NegativeOffset_Fails()
    {
        var validator = new GetSessionHistoryQueryValidator();
        var query = new GetSessionHistoryQuery("user-1", Limit: 10, Offset: -1);

        var result = await validator.ValidateAsync(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Offset");
    }
}
