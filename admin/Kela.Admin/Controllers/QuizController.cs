using FluentValidation;
using Kela.Core.Common.Bulk;
using Kela.Core.Common.Sorting;
using Kela.Core.Modules.Lesson.Abstractions;
using Kela.Core.Modules.Lesson.Requests;
using Kela.Core.Modules.Question.Abstractions;
using Kela.Core.Modules.Question.Requests;
using Kela.Core.Modules.Quiz.Abstractions;
using Kela.Core.Modules.Quiz.Requests;
using Kela.Core.Modules.Subject.Abstractions;
using Kela.Core.Modules.Subject.Requests;
using Microsoft.AspNetCore.Mvc;
using Kela.Admin.Infrastructure;
using Kela.Core.Common.Requests;

namespace Kela.Admin.Controllers;

[Route("[controller]")]
public sealed class QuizController(
    IQuizService service,
    ILessonService lessons,
    IQuestionService questions,
    ISubjectService subjects,
    IValidator<QuizUpsertRequest> validator) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Scripts"] = new[]
        {
            "~/js/pages/quiz/list.js", "~/js/pages/quiz/create.js",
            "~/js/pages/quiz/update.js", "~/js/pages/quiz/questions.js",
            "~/js/pages/quiz/groups.js", "~/js/pages/quiz/sort.js"
        };
        ViewData["Lessons"] = await lessons.ListAsync(
            new LessonListRequest { PerPage = 500, UserId = User.IsAdmin() ? null : User.CurrentUserId() }, ct);
        ViewData["Subjects"] = await subjects.ListAsync(
            new SubjectListRequest { PerPage = 500, UserId = User.IsAdmin() ? null : User.CurrentUserId() }, ct);
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] QuizListRequest request, CancellationToken ct)
    {
        if (!User.IsAdmin())
        {
            // Teacher yalnız öz qeydlərini görür.
            request.IsDeleted = 0;
            request.UserId = User.CurrentUserId();
        }
        return Json(await service.ListAsync(request, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(await service.GetAsync(id, OwnerScope, ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] QuizUpsertRequest request, CancellationToken ct)
    {
        // Teacher yaratdığı qeydin sahibi olur (özü görə bilsin).
        if (!User.IsAdmin()) request.UserId = User.CurrentUserId();

        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Invalid(result));

        return Json(await service.CreateAsync(request, ct));
    }

    [HttpPut("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long id, [FromForm] QuizUpsertRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Invalid(result));

        return Json(await service.UpdateAsync(id, request, OwnerScope, ct));
    }

    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, OwnerScope, ct);
        return Json(new { ok = true });
    }

    [HttpPost("sort")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sort([FromBody] SortRequest request, CancellationToken ct)
    {
        await service.ReorderAsync(request, OwnerScope, ct);
        return Json(new { ok = true });
    }

    [HttpPut("{id:long}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(long id, [FromBody] SetActiveRequest request, CancellationToken ct)
    {
        await service.SetActiveAsync(id, request.IsActive, OwnerScope, ct);
        return Json(new { ok = true });
    }

    [HttpPost("bulk")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Bulk([FromBody] BulkRequest request, CancellationToken ct)
        => BulkAsync(request, service.BulkAsync, ct);

    // --- Suallar ---

    [HttpGet("options")]
    public async Task<IActionResult> Options([FromQuery(Name = "quiz_id")] long? quizId, CancellationToken ct)
    {
        // İmtahan hansı dərsə/fənnə bağlıdırsa yalnız həmin fənnin sualları təklif olunur.
        long? subjectId = null;

        if (quizId is long id)
        {
            var quiz = await service.GetAsync(id, OwnerScope, ct);

            if (quiz.LessonId is long lessonId)
            {
                subjectId = (await lessons.GetAsync(lessonId, OwnerScope, ct)).SubjectId;
            }
        }

        var list = await questions.ListAsync(new QuestionListRequest
        {
            PerPage = 500,
            SubjectId = subjectId,
            UserId = User.IsAdmin() ? null : User.CurrentUserId()
        }, ct);

        // Şəkilli sualın başlığı boş olur — seçimdə hansı sual olduğunu görmək üçün
        // şəkil URL-ləri də qaytarılır (modal onları önizləmə kimi göstərir).
        return Json(list.Data.Select(q => new
        {
            id = q.Id,
            // Şəkilli sualda Title URL/path saxlayır — seçimdə onu göstərmirik.
            text = q.TitleType == "image" || string.IsNullOrWhiteSpace(q.Title) ? $"#{q.Id}" : q.Title,
            titleUrl = q.TitleType == "image" ? q.TitleUrl : null,
            imageUrls = new[] { q.VariantAUrl, q.VariantBUrl, q.VariantCUrl, q.VariantDUrl, q.VariantEUrl }
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .ToList()
        }));
    }

    [HttpGet("{id:long}/questions")]
    public async Task<IActionResult> QuizQuestions(long id, CancellationToken ct)
        => Json(await service.ListQuestionsAsync(id, OwnerScope, ct));

    [HttpPost("{id:long}/questions")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignQuestions(long id, [FromBody] AssignQuestionsRequest request, CancellationToken ct)
        => Json(await service.AssignQuestionsAsync(id, request, OwnerScope, ct));

    // --- Qruplar ---

    [HttpGet("{id:long}/groups")]
    public async Task<IActionResult> Groups(long id, CancellationToken ct)
        => Json(await service.ListGroupsAsync(id, OwnerScope, ct));

    [HttpGet("{id:long}/group-options")]
    public async Task<IActionResult> GroupOptions(long id, CancellationToken ct)
        => Json(await service.ListGroupOptionsAsync(id, OwnerScope, ct));

    [HttpPost("{id:long}/groups")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignGroups(
        long id, [FromBody] AssignGroupsRequest request, CancellationToken ct)
        => Json(await service.AssignGroupsAsync(id, request, OwnerScope, ct));

    [HttpDelete("{id:long}/groups/{groupId:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveGroup(long id, long groupId, CancellationToken ct)
    {
        await service.RemoveGroupAsync(id, groupId, OwnerScope, ct);
        return Json(new { ok = true });
    }

    [HttpDelete("{id:long}/questions/{questionId:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveQuestion(long id, long questionId, CancellationToken ct)
    {
        await service.RemoveQuestionAsync(id, questionId, OwnerScope, ct);
        return Json(new { ok = true });
    }

}
