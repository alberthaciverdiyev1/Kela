using LanguageLMS.Core.Modules.Group.Responses;
using LanguageLMS.Core.Modules.Lesson.Responses;
using LanguageLMS.Core.Modules.LessonContent.Responses;
using LanguageLMS.Core.Modules.Quiz.Responses;
using LanguageLMS.Core.Modules.Subject.Responses;

namespace LanguageLMS.Core.Modules.StudentApp.Services;

public record ContentCountResponse(int Video, int File, int Voice, int Quiz);

public interface IStudentAppService
{
    Task<IReadOnlyList<GroupResponse>> ListGroupsAsync(long studentId, CancellationToken ct = default);

    Task<IReadOnlyList<SubjectResponse>> ListSubjectsAsync(long studentId, long groupId, CancellationToken ct = default);

    Task<IReadOnlyList<LessonResponse>> ListLessonsAsync(long studentId, long subjectId, CancellationToken ct = default);

    Task<IReadOnlyList<LessonContentResponse>> ListContentsAsync(long studentId, long lessonId, CancellationToken ct = default);

    Task<ContentCountResponse> GetContentCountAsync(long studentId, long lessonId, CancellationToken ct = default);

    Task<IReadOnlyList<QuizResponse>> ListQuizzesAsync(long studentId, CancellationToken ct = default);
}
