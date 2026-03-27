using Exit.exe.Application.Features.Sessions.Queries;
using FluentValidation;

namespace Exit.exe.Application.Features.Sessions.Validators;

public sealed class GetSessionHistoryQueryValidator : AbstractValidator<GetSessionHistoryQuery>
{
    public GetSessionHistoryQueryValidator()
    {
        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Limit must not exceed 100.");

        RuleFor(x => x.Offset)
            .GreaterThanOrEqualTo(0).WithMessage("Offset must be greater than or equal to 0.");
    }
}
