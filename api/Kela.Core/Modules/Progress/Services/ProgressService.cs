using Kela.Core.Common.Exceptions;
using Kela.Core.Common.Storage;
using Kela.Core.Data;
using Kela.Core.Modules.Progress.Requests;
using Kela.Core.Modules.Progress.Responses;
using Kela.Core.Modules.StudentApp.Services;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Progress.Services;

using LessonProgress = Kela.Core.Modules.Progress.Entities.LessonProgress;
using LessonContent = Kela.Core.Modules.LessonContent.Entities.LessonContent;

public sealed class ProgressService(
    AppDbContext db,
    IFileStorage storage,
    IStudentAccessService access) : IProgressService
{
    public async Task<MarkProgressResponse> MarkCompletedAsync(
        long studentId, MarkProgressRequest request, CancellationToken ct = default)
    {
        var content = await db.LessonContents.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ContentId, ct)
            ?? throw new KeyNotFoundException($"Lesson content not found: {request.ContentId}");

        var lesson = await db.Lessons.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == content.LessonId, ct)
            ?? throw new KeyNotFoundException($"Lesson not found: {content.LessonId}");

        if (!await access.HasSubjectAccessAsync(studentId, lesson.SubjectId, ct))
        {
            throw new ForbiddenException("You do not have access to this content");
        }

        var existing = await db.LessonProgress
            .IgnoreQueryFilters(new[] { "SoftDelete" })
            .FirstOrDefaultAsync(x => x.StudentId == studentId && x.LessonContentId == request.ContentId, ct);

        if (existing is null)
        {
            db.LessonProgress.Add(new LessonProgress
            {
                StudentId = studentId,
                LessonContentId = request.ContentId,
                Status = "completed",
                CompletedAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            existing.DeletedAt = null;
            existing.Status = "completed";
            existing.CompletedAt ??= DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        return new MarkProgressResponse(request.ContentId, "completed");
    }

    public async Task<ProgressResponse> GetProgressAsync(long studentId, CancellationToken ct = default)
    {
        var subjectIds = await access.AccessibleSubjectIdsAsync(studentId, ct);

        var lessons = await db.Lessons.AsNoTracking()
            .Where(l => subjectIds.Contains(l.SubjectId) && l.IsActive)
            .OrderBy(l => l.SortOrder).ThenBy(l => l.Id)
            .ToListAsync(ct);

        var lessonIds = lessons.Select(l => l.Id).ToList();

        var contents = await db.LessonContents.AsNoTracking()
            .Where(c => lessonIds.Contains(c.LessonId) && c.IsActive)
            .Include(c => c.Video).Include(c => c.MediaFile).Include(c => c.VoiceRecord)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Id)
            .ToListAsync(ct);

        var completed = (await db.LessonProgress
            .Where(p => p.StudentId == studentId)
            .Select(p => p.LessonContentId)
            .ToListAsync(ct)).ToHashSet();

        var items = lessons.Select(lesson =>
        {
            var lessonContents = contents.Where(c => c.LessonId == lesson.Id).ToList();
            var total = lessonContents.Count;
            var done = lessonContents.Count(c => completed.Contains(c.Id));
            var next = lessonContents.FirstOrDefault(c => !completed.Contains(c.Id));

            return new ProgressItemResponse(
                lesson.Id,
                lesson.Title,
                storage.GetUrl(lesson.Image),
                total,
                done,
                total > 0 ? (int)Math.Round(done * 100.0 / total) : 0,
                next is null ? null : new ProgressContentRef(next.Id, next.Title, ContentType(next)));
        }).ToList();

        return new ProgressResponse("lesson", items);
    }

    public async Task<StatisticsResponse> GetStatisticsAsync(long studentId, CancellationToken ct = default)
    {
        var subjectIds = await access.AccessibleSubjectIdsAsync(studentId, ct);

        var lessons = await db.Lessons.AsNoTracking()
            .Where(l => subjectIds.Contains(l.SubjectId) && l.IsActive)
            .ToListAsync(ct);

        var lessonIds = lessons.Select(l => l.Id).ToList();

        var contents = await db.LessonContents.AsNoTracking()
            .Where(c => lessonIds.Contains(c.LessonId) && c.IsActive)
            .ToListAsync(ct);

        var completed = (await db.LessonProgress
            .Where(p => p.StudentId == studentId)
            .Select(p => p.LessonContentId)
            .ToListAsync(ct)).ToHashSet();

        var totalContents = contents.Count;
        var completedContents = contents.Count(c => completed.Contains(c.Id));
        var overview = new StatisticsOverview(
            lessons.Count,
            totalContents,
            completedContents,
            Percent(completedContents, totalContents));

        var subjects = await db.Subjects.AsNoTracking()
            .Where(s => subjectIds.Contains(s.Id))
            .ToListAsync(ct);

        var bySubject = subjects.Select(subject =>
        {
            var subjectLessonIds = lessons.Where(l => l.SubjectId == subject.Id).Select(l => l.Id).ToHashSet();
            var subjectContents = contents.Where(c => subjectLessonIds.Contains(c.LessonId)).ToList();
            var done = subjectContents.Count(c => completed.Contains(c.Id));

            return new SubjectStatResponse(
                subject.Id,
                subject.Name,
                subjectContents.Count,
                done,
                Percent(done, subjectContents.Count));
        }).ToList();

        return new StatisticsResponse(overview, bySubject);
    }

    private static int Percent(int part, int total) => total > 0 ? (int)Math.Round(part * 100.0 / total) : 0;

    private static string? ContentType(LessonContent content)
    {
        if (content.Video is not null) return "video";
        if (content.MediaFile is not null) return "file";
        if (content.VoiceRecord is not null) return "voice";
        return null;
    }
}
