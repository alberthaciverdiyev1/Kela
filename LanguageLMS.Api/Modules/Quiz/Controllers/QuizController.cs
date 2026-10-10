using LanguageLMS.Api.Common.Authorization;
using LanguageLMS.Core.Common.Bulk;
using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.Question.Responses;
using LanguageLMS.Core.Modules.Quiz.Requests;
using LanguageLMS.Core.Modules.Quiz.Responses;
using LanguageLMS.Core.Modules.Quiz.Services;
using LanguageLMS.Core.Modules.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Quiz.Controllers;

[Authorize(Policy = AuthPolicies.Teacher)]
[HasPermission("quiz.manage")]
[Route("api/[controller]")]
public sealed class QuizController(IQuizService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<QuizResponse>>>> List(
        [FromQuery] QuizListRequest request, CancellationToken ct)
        => Success(await service.ListAsync(request, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<QuizResponse>>> Show(long id, CancellationToken ct)
        => Success(await service.GetAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<QuizResponse>>> Add(
        [FromForm] QuizUpsertRequest request, CancellationToken ct)
        => Success(await service.CreateAsync(request, ct), "Created", StatusCodes.Status201Created);

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<QuizResponse>>> Update(
        long id, [FromForm] QuizUpsertRequest request, CancellationToken ct)
        => Success(await service.UpdateAsync(id, request, ct), "Updated");

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return SuccessMessage("Deleted");
    }

    [HttpGet("{id:long}/questions")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<QuestionResponse>>>> ListQuestions(long id, CancellationToken ct)
        => Success(await service.ListQuestionsAsync(id, ct));

    [HttpPost("{id:long}/questions")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<QuestionResponse>>>> AssignQuestions(
        long id, [FromBody] AssignQuestionsRequest request, CancellationToken ct)
        => Success(await service.AssignQuestionsAsync(id, request, ct), "Questions assigned");

    [HttpDelete("{id:long}/questions/{questionId:long}")]
    public async Task<ActionResult<ApiResponse<string>>> RemoveQuestion(long id, long questionId, CancellationToken ct)
    {
        await service.RemoveQuestionAsync(id, questionId, ct);
        return SuccessMessage("Question removed");
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<ApiResponse<string>>> Bulk([FromBody] BulkRequest request, CancellationToken ct)
    {
        await service.BulkAsync(request, ct);
        return SuccessMessage("Bulk action applied");
    }
}
