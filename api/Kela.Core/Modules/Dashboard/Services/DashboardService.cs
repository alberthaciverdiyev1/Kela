using Kela.Core.Data;
using Kela.Core.Modules.Dashboard.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Dashboard.Services;

public sealed class DashboardService(AppDbContext db) : Abstractions.IDashboardService
{
    public async Task<DashboardResponse> GetAsync(long? ownerId = null, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var from30 = now.AddDays(-30);

        var studentsQuery = db.Students.AsQueryable();
        var groupsQuery = db.Groups.AsQueryable();
        var subjectsQuery = db.Subjects.AsQueryable();
        var lessonsQuery = db.Lessons.AsQueryable();
        var contentsQuery = db.LessonContents.AsQueryable();
        var quizzesQuery = db.Quizzes.AsQueryable();
        var questionsQuery = db.Questions.AsQueryable();
        var attemptsQuery = db.StudentQuizzes.AsQueryable();

        if (ownerId is long owner)
        {
            // Teacher yalnız öz qruplarının şagirdlərini və öz/admin məzmununu görür.
            var studentIds = db.StudentGroups
                .Where(sg => db.Groups.Any(g => g.Id == sg.GroupId && g.UserId == owner))
                .Select(sg => sg.StudentId);

            studentsQuery = studentsQuery.Where(x => studentIds.Contains(x.Id));
            groupsQuery = groupsQuery.Where(x => x.UserId == owner);
            subjectsQuery = subjectsQuery.Where(x => x.UserId == owner || x.UserId == null);
            lessonsQuery = lessonsQuery.Where(x => x.UserId == owner || x.UserId == null);
            contentsQuery = contentsQuery.Where(x => x.UserId == owner || x.UserId == null);
            quizzesQuery = quizzesQuery.Where(x => x.UserId == owner || x.UserId == null);
            questionsQuery = questionsQuery.Where(x => x.UserId == owner || x.UserId == null);
            attemptsQuery = attemptsQuery.Where(x => studentIds.Contains(x.StudentId));
        }

        var students = new DashboardStudents(
            await studentsQuery.CountAsync(ct),
            await studentsQuery.CountAsync(x => x.IsActive, ct),
            await studentsQuery.CountAsync(x => x.CreatedAt >= now.AddDays(-7), ct),
            await studentsQuery.CountAsync(x => x.CreatedAt >= from30, ct),
            await groupsQuery.CountAsync(ct));

        var content = new DashboardContent(
            await subjectsQuery.CountAsync(ct),
            await lessonsQuery.CountAsync(ct),
            await contentsQuery.CountAsync(ct),
            await contentsQuery.Where(x => x.VideoId != null).Select(x => x.VideoId).Distinct().CountAsync(ct),
            await contentsQuery.Where(x => x.MediaFileId != null).Select(x => x.MediaFileId).Distinct().CountAsync(ct),
            await contentsQuery.Where(x => x.VoiceRecordId != null).Select(x => x.VoiceRecordId).Distinct().CountAsync(ct),
            await quizzesQuery.CountAsync(ct),
            await questionsQuery.CountAsync(ct));

        var activity = new DashboardActivity(
            await attemptsQuery.CountAsync(x => x.CreatedAt >= from30, ct),
            await attemptsQuery.Where(x => x.CreatedAt >= from30).Select(x => x.StudentId).Distinct().CountAsync(ct));

        var lessonCounts = await lessonsQuery
            .AsNoTracking()
            .GroupBy(l => l.SubjectId)
            .Select(g => new { SubjectId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync(ct);

        var subjectIds = lessonCounts.Select(x => x.SubjectId).ToList();
        var subjectNames = await subjectsQuery.AsNoTracking()
            .Where(s => subjectIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name, ct);

        var topSubjects = lessonCounts
            .Select(x => new TopSubject(x.SubjectId, subjectNames.GetValueOrDefault(x.SubjectId, string.Empty), x.Count))
            .ToList();

        var recentStudents = await studentsQuery
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Take(5)
            .Select(x => new RecentStudent(x.Id, x.FullName, x.CreatedAt))
            .ToListAsync(ct);

        return new DashboardResponse(students, content, activity, topSubjects, recentStudents);
    }
}
