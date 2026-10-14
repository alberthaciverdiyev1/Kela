using FluentValidation;
using Kela.Core.Common.Sorting;
using Kela.Core.Modules.Lesson.Abstractions;
using Kela.Core.Modules.Lesson.Requests;
using Kela.Core.Modules.Question.Abstractions;
using Kela.Core.Modules.Question.Requests;
using Kela.Core.Modules.Quiz.Abstractions;
using Kela.Core.Modules.Quiz.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Mvc.Controllers;

[Route("[controller]")]
public sealed class QuizController(
    IQuizService service,
    ILessonService lessons,
    IQuestionService questions,
    IValidator<QuizUpsertRequest> validator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "İmtahanlar";
        ViewData["Scripts"] = new[]
        {
            "~/js/pages/quiz/list.js", "~/js/pages/quiz/create.js",
            "~/js/pages/quiz/update.js", "~/js/pages/quiz/questions.js",
            "~/js/pages/quiz/sort.js"
        };
        ViewData["Lessons"] = await lessons.ListAsync(new LessonListRequest { PerPage = 500 }, ct);
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] QuizListRequest request, CancellationToken ct)
        => Json(await service.ListAsync(request, ct));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(await service.GetAsync(id, ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] QuizUpsertRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Errors(result));

        return Json(await service.CreateAsync(request, ct));
    }

    [HttpPut("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long id, [FromForm] QuizUpsertRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Errors(result));

        return Json(await service.UpdateAsync(id, request, ct));
    }

    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Json(new { ok = true });
    }

    [HttpPost("sort")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sort([FromBody] SortRequest request, CancellationToken ct)
    {
        await service.ReorderAsync(request, ct);
        return Json(new { ok = true });
    }

    // --- Suallar ---

    [HttpGet("options")]
    public async Task<IActionResult> Options(CancellationToken ct)
    {
        var list = await questions.ListAsync(new QuestionListRequest { PerPage = 500 }, ct);
        return Json(list.Data.Select(q => new { id = q.Id, text = string.IsNullOrWhiteSpace(q.Title) ? $"#{q.Id}" : q.Title }));
    }

    [HttpGet("{id:long}/questions")]
    public async Task<IActionResult> QuizQuestions(long id, CancellationToken ct)
        => Json(await service.ListQuestionsAsync(id, ct));

    [HttpPost("{id:long}/questions")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignQuestions(long id, [FromBody] AssignQuestionsRequest request, CancellationToken ct)
        => Json(await service.AssignQuestionsAsync(id, request, ct));

    [HttpDelete("{id:long}/questions/{questionId:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveQuestion(long id, long questionId, CancellationToken ct)
    {
        await service.RemoveQuestionAsync(id, questionId, ct);
        return Json(new { ok = true });
    }

    private static object Errors(FluentValidation.Results.ValidationResult result)
        => new
        {
            errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
        };
}
