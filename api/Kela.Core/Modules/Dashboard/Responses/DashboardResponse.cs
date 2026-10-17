namespace Kela.Core.Modules.Dashboard.Responses;

public record DashboardStudents(int Total, int Active, int New7d, int New30d, int Groups);

public record DashboardContent(
    int Subjects, int Lessons, int Contents, int Videos, int Files, int Voices, int Quizzes, int Questions);

public record DashboardActivity(int QuizAttempts30d, int StudentsAttempted30d);

public record TopSubject(long SubjectId, string Name, int LessonCount);

public record RecentStudent(long Id, string FullName, DateTimeOffset CreatedAt);

public record DashboardResponse(
    DashboardStudents Students,
    DashboardContent Content,
    DashboardActivity Activity,
    IReadOnlyList<TopSubject> TopSubjects,
    IReadOnlyList<RecentStudent> RecentStudents);
