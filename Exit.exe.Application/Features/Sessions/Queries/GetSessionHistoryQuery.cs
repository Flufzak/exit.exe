using Exit.exe.Application.Contracts;
using Exit.exe.Application.Features.Sessions.DTOs;
using MediatR;

namespace Exit.exe.Application.Features.Sessions.Queries;

public sealed record GetSessionHistoryQuery(string UserId) : IRequest<IReadOnlyList<SessionSummaryDto>>;

public sealed class GetSessionHistoryQueryHandler(
    ISessionRepository sessionRepository) : IRequestHandler<GetSessionHistoryQuery, IReadOnlyList<SessionSummaryDto>>
{
    public async Task<IReadOnlyList<SessionSummaryDto>> Handle(
        GetSessionHistoryQuery request,
        CancellationToken cancellationToken)
    {
        return await sessionRepository.GetHistoryByUserAsync(request.UserId, cancellationToken);
    }
}
