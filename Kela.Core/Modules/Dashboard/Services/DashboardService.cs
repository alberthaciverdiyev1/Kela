using Kela.Core.Data;
using Kela.Core.Modules.Dashboard.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Dashboard.Services;

public sealed class DashboardService(AppDbContext db) : Abstractions.IDashboardService
{
    public async Task<DashboardResponse> GetAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var from30 = now.AddDays(-30);

        var students = new DashboardStudents(
            await db.Students.CountAsync(ct),
            await db.Students.CountAsync(x => x.IsActive, ct),
            await db.Students.CountAsync(x => x.CreatedAt >= now.AddDays(-7), ct),
            await db.Students.CountAsync(x => x.CreatedAt >= from30, ct),
            await db.Groups.CountAsync(ct));

        var content = new DashboardContent(
            await db.Subjects.CountAsync(ct),
            await db.Lessons.CountAsync(ct),
            await db.LessonContents.CountAsync(ct),
            await db.Videos.CountAsync(ct),
            await db.MediaFiles.CountAsync(ct),
            await db.VoiceRecords.CountAsync(ct),
            await db.Quizzes.CountAsync(ct),
            await db.Questions.CountAsync(ct));

        var activity = new DashboardActivity(
            await db.StudentQuizzes.CountAsync(x => x.CreatedAt >= from30, ct),
            await db.StudentQuizzes.Where(x => x.CreatedAt >= from30).Select(x => x.StudentId).Distinct().CountAsync(ct));

        var lessonCounts = await db.Lessons
            .AsNoTracking()
            .GroupBy(l => l.SubjectId)
            .Select(g => new { SubjectId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync(ct);

        var subjectIds = lessonCounts.Select(x => x.SubjectId).ToList();
        var subjectNames = await db.Subjects.AsNoTracking()
            .Where(s => subjectIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name, ct);

        var topSubjects = lessonCounts
            .Select(x => new TopSubject(x.SubjectId, subjectNames.GetValueOrDefault(x.SubjectId, string.Empty), x.Count))
            .ToList();

        var recentStudents = await db.Students
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Take(5)
            .Select(x => new RecentStudent(x.Id, x.FullName, x.CreatedAt))
            .ToListAsync(ct);

        return new DashboardResponse(students, content, activity, topSubjects, recentStudents);
    }
}
