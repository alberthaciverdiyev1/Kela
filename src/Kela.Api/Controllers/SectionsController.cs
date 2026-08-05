using Kela.Application.Sections.Commands.CreateSection;
using Kela.Application.Sections.Commands.DeleteSection;
using Kela.Application.Sections.Commands.UpdateSection;
using Kela.Application.Sections.Queries.GetSectionById;
using Kela.Application.Sections.Queries.GetSections;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Api.Controllers;

[ApiController]
[Route("api/sections")]
public sealed class SectionsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await sender.Send(new GetSectionsQuery(page, pageSize), cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var section = await sender.Send(new GetSectionByIdQuery(id), cancellationToken);
        return section is null ? NotFound() : Ok(section);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSectionCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateSectionCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command with { Id = id }, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteSectionCommand(id), cancellationToken);
        return NoContent();
    }
}
