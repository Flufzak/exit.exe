using MediatR;
using Exit.exe.Application.Features.Stories.StoryDtos;

namespace Exit.exe.Application.Features.Stories.Queries;

public sealed record GetStoriesQuery() 
    : IRequest<IReadOnlyList<StoryDto>>;


public sealed class GetStoriesQueryHandler 
    : IRequestHandler<GetStoriesQuery, IReadOnlyList<StoryDto>>
{
    public Task<IReadOnlyList<StoryDto>> Handle(
        GetStoriesQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<StoryDto> stories =
        [
            new StoryDto(
                "kazimir",
                "Kazimir",
                "A medieval cult escape experience.",
                "60 min",
                "Hard",
                "Live",
                "available"
            ),
            new StoryDto(
                "abyss",
                "The Abyss",
                "Something is awakening below.",
                "75 min",
                "Extreme",
                "Coming Soon",
                "upcoming"
            )
        ];

        return Task.FromResult(stories);
    }
}