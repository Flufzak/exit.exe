using Exit.exe.Application.Features.Stories.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Exit.exe.Web.Controllers;

[ApiController]
[Route("api/stories")]
public sealed class StoriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await sender.Send(new GetStoriesQuery(), ct);
        return Ok(result);
    }
}