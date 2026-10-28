using Edutor.Core.Modules.Group.Responses;
using Edutor.Core.Modules.Lesson.Responses;
using Edutor.Core.Modules.LessonContent.Responses;
using Edutor.Core.Modules.Quiz.Responses;
using Edutor.Core.Modules.StudentApp.Responses;
using Edutor.Core.Modules.Subject.Responses;

namespace Edutor.Core.Modules.StudentApp.Abstractions;

public record ContentCountResponse(int TopicExplain, int LiveVideo, int Pdf, int VoiceRecord, int TopicQuiz, int GeneralQuiz);

public interface IStudentAppService
{
    Task<IReadOnlyList<StudentGroupResponse>> ListGroupsAsync(long studentId, CancellationToken ct = default);

    /// <summary>Dəvət linki ilə qrup məlumatı və şagirdin müraciət vəziyyəti.</summary>
    Task<GroupJoinInfoResponse> GetGroupJoinInfoAsync(long studentId, string token, CancellationToken ct = default);

    /// <summary>Dəvət linki üçün public məlumat (giriş etməmiş şagird qeydiyyat üçün tenant-ı görsün).</summary>
    Task<GroupJoinInfoResponse> GetPublicGroupJoinInfoAsync(string token, CancellationToken ct = default);

    /// <summary>Dəvət linki ilə qrupa qoşulma müraciəti yaradır (teacher təsdiqi gözləyir).</summary>
    Task<GroupJoinInfoResponse> RequestJoinGroupAsync(long studentId, string token, CancellationToken ct = default);

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
