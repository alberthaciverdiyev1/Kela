using System.Security.Claims;
using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Modules.Auth;
using LanguageLMS.Core.Modules.Group.Responses;
using LanguageLMS.Core.Modules.Lesson.Responses;
using LanguageLMS.Core.Modules.LessonContent.Responses;
using LanguageLMS.Core.Modules.Quiz.Responses;
using LanguageLMS.Core.Modules.StudentApp.Services;
using LanguageLMS.Core.Modules.Subject.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.StudentApp.Controllers;

[Authorize(Policy = AuthPolicies.Student)]
[Route("api/[controller]")]
public sealed class StudentController(IStudentAppService service) : ApiControllerBase
{
    private long StudentId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("groups")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GroupResponse>>>> Groups(CancellationToken ct)
        => Success(await service.ListGroupsAsync(StudentId, ct));

    [HttpGet("groups/{groupId:long}/subjects")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SubjectResponse>>>> Subjects(long groupId, CancellationToken ct)
        => Success(await service.ListSubjectsAsync(StudentId, groupId, ct));

    [HttpGet("subjects/{subjectId:long}/lessons")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LessonResponse>>>> Lessons(long subjectId, CancellationToken ct)
        => Success(await service.ListLessonsAsync(StudentId, subjectId, ct));

    [HttpGet("lessons/{lessonId:long}/contents")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LessonContentResponse>>>> Contents(long lessonId, CancellationToken ct)
        => Success(await service.ListContentsAsync(StudentId, lessonId, ct));

    [HttpGet("quizzes")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<QuizResponse>>>> Quizzes(CancellationToken ct)
        => Success(await service.ListQuizzesAsync(StudentId, ct));
}
