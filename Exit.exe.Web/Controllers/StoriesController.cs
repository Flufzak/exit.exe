using MediatR;
using Microsoft.AspNetCore.Mvc;
using Exit.exe.Application.Features.Stories.Queries;

namespace Exit.exe.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public StoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var stories = await _mediator.Send(new GetStoriesQuery());
        return Ok(stories);
    }
}