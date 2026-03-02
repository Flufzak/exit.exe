using MediatR;

namespace Exit.exe.Application.Features.Games.Queries;

// Query (request) that returns the available games for the MVP.
public sealed record GetGamesQuery() : IRequest<IReadOnlyList<GameListItem>>;

// Simple DTO for the list (no entities yet).
public sealed record GameListItem(string Code, string DisplayName);

// Handler for the query.
// For now we return a hardcoded list to keep the slice minimal.
public sealed class GetGamesQueryHandler : IRequestHandler<GetGamesQuery, IReadOnlyList<GameListItem>>
{
    public Task<IReadOnlyList<GameListItem>> Handle(GetGamesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<GameListItem> games =
        [
            new GameListItem("hangman", "Hangman")
        ];

        return Task.FromResult(games);
    }
}