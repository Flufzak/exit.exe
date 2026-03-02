namespace Exit.exe.Application.Features.Stories.StoryDtos;

public sealed record StoryDto(
    string Id,
    string Title,
    string Description,
    string? Duration,
    string? Difficulty,
    string? Status,
    string Type
);