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
                "Bound in the depths of a forgotten monastery, you are chosen for a ritual that promises immortality. The priests believe destiny has led you here. Prove them wrong.",
                "60 min",
                "Hard",
                "Live",
                "available"
            ),
            new StoryDto(
                "abyss",
                "The Abyss",
                "The descent was easy. The return will not be. The Abyss does not reveal its secrets without taking something in return.",
                "75 min",
                "Extreme",
                "Coming Soon",
                "upcoming"
            )
        ];

        return Task.FromResult(stories);
    }
}