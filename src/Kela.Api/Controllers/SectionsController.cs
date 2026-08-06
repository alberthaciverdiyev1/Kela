using Kela.Application.Sections;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Api.Controllers;

[ApiController]
[Route("api/sections")]
public sealed class SectionsController(ISectionService sections) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await sections.GetPageAsync(page, pageSize, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var section = await sections.GetByIdAsync(id, cancellationToken);
        return section is null ? NotFound() : Ok(section);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSectionRequest request,
        CancellationToken cancellationToken)
    {
        var id = await sections.CreateAsync(request.Name, request.Level, request.TeacherId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateSectionRequest request,
        CancellationToken cancellationToken)
    {
        await sections.UpdateAsync(id, request.Name, request.Level, request.TeacherId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await sections.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    public sealed record CreateSectionRequest(string Name, int Level, int? TeacherId);
    public sealed record UpdateSectionRequest(string Name, int Level, int? TeacherId);
}
