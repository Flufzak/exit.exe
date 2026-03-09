using Exit.exe.Application.Features.Sessions.Commands;
using FluentValidation;

namespace Exit.exe.Application.Features.Sessions.Validators;

public sealed class SubmitGuessCommandValidator : AbstractValidator<SubmitGuessCommand>
{
    public SubmitGuessCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty().WithMessage("Session ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
        RuleFor(x => x.Letter)
            .NotEmpty().WithMessage("Letter is required.")
            .Length(1).WithMessage("Guess must be a single letter.")
            .Matches("^[a-zA-Z]$").WithMessage("Guess must be a letter.");
    }
}
