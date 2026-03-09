using Exit.exe.Application.Behaviors;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Exit.exe.Application.Tests;

public class ValidationBehaviorTests
{
    private record TestRequest(string Value) : IRequest<string>;

    private sealed class PassingValidator : AbstractValidator<TestRequest>
    {
        public PassingValidator() => RuleFor(r => r.Value).NotEmpty();
    }

    private sealed class FailingValidator : AbstractValidator<TestRequest>
    {
        public FailingValidator() => RuleFor(r => r.Value).Must(_ => false).WithMessage("Always fails");
    }

    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([]);
        var nextCalled = false;
        RequestHandlerDelegate<string> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult("ok");
        };

        var result = await behavior.Handle(new TestRequest("hello"), next, CancellationToken.None);

        Assert.True(nextCalled);
        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_AllValidatorsPass_CallsNext()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([new PassingValidator()]);
        var nextCalled = false;
        RequestHandlerDelegate<string> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult("ok");
        };

        var result = await behavior.Handle(new TestRequest("hello"), next, CancellationToken.None);

        Assert.True(nextCalled);
        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_ValidatorFails_ThrowsValidationException()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([new FailingValidator()]);
        RequestHandlerDelegate<string> next = _ => Task.FromResult("ok");

        await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(new TestRequest("hello"), next, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidatorFails_NextNotCalled()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([new FailingValidator()]);
        var nextCalled = false;
        RequestHandlerDelegate<string> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult("ok");
        };

        await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(new TestRequest("hello"), next, CancellationToken.None));

        Assert.False(nextCalled);
    }

    [Fact]
    public async Task Handle_CancellationToken_PassedToValidatorsAndNext()
    {
        using var cts = new CancellationTokenSource();
        var tokenSeenByValidator = CancellationToken.None;
        var tokenSeenByNext = CancellationToken.None;

        var validator = new InlineValidator<TestRequest>();
        validator.RuleFor(r => r.Value).CustomAsync(async (_, ctx, ct) =>
        {
            tokenSeenByValidator = ct;
            await Task.CompletedTask;
        });

        var behavior = new ValidationBehavior<TestRequest, string>([validator]);
        RequestHandlerDelegate<string> next = ct =>
        {
            tokenSeenByNext = ct;
            return Task.FromResult("ok");
        };

        await behavior.Handle(new TestRequest("hello"), next, cts.Token);

        Assert.Equal(cts.Token, tokenSeenByValidator);
        Assert.Equal(cts.Token, tokenSeenByNext);
    }

    [Fact]
    public async Task Handle_CancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var behavior = new ValidationBehavior<TestRequest, string>([new PassingValidator()]);
        RequestHandlerDelegate<string> next = ct =>
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult("ok");
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => behavior.Handle(new TestRequest("hello"), next, cts.Token));
    }
}
