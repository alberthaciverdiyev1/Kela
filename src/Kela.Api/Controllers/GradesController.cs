using Kela.Application.Grades.Commands.CreateGrade;
using Kela.Application.Grades.Commands.DeleteGrade;
using Kela.Application.Grades.Commands.UpdateGrade;
using Kela.Application.Grades.Queries.GetGradeById;
using Kela.Application.Grades.Queries.GetGrades;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Api.Controllers;

[ApiController]
[Route("api/grades")]
public sealed class GradesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetGradesQuery(), cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var grade = await sender.Send(new GetGradeByIdQuery(id), cancellationToken);
        return grade is null ? NotFound() : Ok(grade);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateGradeCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateGradeCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command with { Id = id }, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteGradeCommand(id), cancellationToken);
        return NoContent();
    }
}
