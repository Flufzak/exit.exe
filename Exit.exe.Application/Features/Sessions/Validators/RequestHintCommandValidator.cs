using Exit.exe.Application.Features.Sessions.Commands;
using FluentValidation;

namespace Exit.exe.Application.Features.Sessions.Validators;

public sealed class RequestHintCommandValidator : AbstractValidator<RequestHintCommand>
{
    public RequestHintCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty().WithMessage("Session ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
    }
}
