using Exit.exe.Application.Features.Sessions.Commands;
using FluentValidation;

namespace Exit.exe.Application.Features.Sessions.Validators;

public sealed class StartSessionCommandValidator : AbstractValidator<StartSessionCommand>
{
    public StartSessionCommandValidator()
    {
        RuleFor(x => x.GameType).NotEmpty().WithMessage("Game type is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
    }
}
