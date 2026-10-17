using Kela.Core.Modules.Group.Responses;
using Kela.Core.Modules.Lesson.Responses;
using Kela.Core.Modules.LessonContent.Responses;
using Kela.Core.Modules.Quiz.Responses;
using Kela.Core.Modules.StudentApp.Responses;
using Kela.Core.Modules.Subject.Responses;

namespace Kela.Core.Modules.StudentApp.Abstractions;

public record ContentCountResponse(int TopicExplain, int LiveVideo, int Pdf, int VoiceRecord, int TopicQuiz, int GeneralQuiz);

public interface IStudentAppService
{
    Task<IReadOnlyList<StudentGroupResponse>> ListGroupsAsync(long studentId, CancellationToken ct = default);

    Task<IReadOnlyList<SubjectResponse>> ListSubjectsAsync(long studentId, long groupId, CancellationToken ct = default);

    /// <summary>Şagirdin bütün qrupları üzrə əlçatan fənlər (filter üçün).</summary>
    Task<IReadOnlyList<SubjectResponse>> ListAllSubjectsAsync(long studentId, CancellationToken ct = default);

    Task<SubjectResponse> GetSubjectAsync(long studentId, long subjectId, CancellationToken ct = default);

    Task<IReadOnlyList<LessonResponse>> ListLessonsAsync(long studentId, long subjectId, CancellationToken ct = default);

    Task<LessonResponse> GetLessonAsync(long studentId, long lessonId, CancellationToken ct = default);

    Task<IReadOnlyList<LessonContentResponse>> ListContentsAsync(long studentId, long lessonId, CancellationToken ct = default);

    Task<ContentCountResponse> GetContentCountAsync(long studentId, long lessonId, CancellationToken ct = default);

    Task<IReadOnlyList<QuizResponse>> ListQuizzesAsync(long studentId, long? subjectId = null, long? lessonId = null, bool general = false, CancellationToken ct = default);

    Task<IReadOnlyList<QuizResponse>> ListLessonQuizzesAsync(long studentId, long lessonId, CancellationToken ct = default);
}
