using FluentValidation;

namespace Exit.exe.Application.Features.Sessions.Commands;

public sealed class StartSessionCommandValidator : AbstractValidator<StartSessionCommand>
{
    public StartSessionCommandValidator()
    {
        RuleFor(x => x.GameType)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}