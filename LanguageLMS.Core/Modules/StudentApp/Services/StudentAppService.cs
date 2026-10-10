using LanguageLMS.Core.Common.Exceptions;
using LanguageLMS.Core.Common.Storage;
using LanguageLMS.Core.Data;
using LanguageLMS.Core.Modules.Group.Responses;
using LanguageLMS.Core.Modules.Lesson.Responses;
using LanguageLMS.Core.Modules.LessonContent.Responses;
using LanguageLMS.Core.Modules.Quiz.Responses;
using LanguageLMS.Core.Modules.Subject.Responses;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Modules.StudentApp.Services;

using LessonContent = LanguageLMS.Core.Modules.LessonContent.Entities.LessonContent;

public sealed class StudentAppService(
    AppDbContext db,
    IFileStorage storage,
    IStudentAccessService access) : IStudentAppService
{
    public async Task<IReadOnlyList<GroupResponse>> ListGroupsAsync(long studentId, CancellationToken ct = default)
    {
        var groups = await db.Groups
            .AsNoTracking()
            .Where(g => db.StudentGroups.Any(sg => sg.GroupId == g.Id && sg.StudentId == studentId))
            .Include(g => g.Memberships)
            .OrderByDescending(g => g.Id)
            .ToListAsync(ct);

        return groups
            .Select(g => new GroupResponse(g.Id, g.Name, g.Description, storage.GetUrl(g.Image), g.UserId, g.Memberships.Count))
            .ToList();
    }

    public async Task<IReadOnlyList<SubjectResponse>> ListSubjectsAsync(
        long studentId, long groupId, CancellationToken ct = default)
    {
        if (!await access.IsGroupMemberAsync(studentId, groupId, ct))
        {
            throw new ForbiddenException("You do not have access to this group");
        }

        return await db.GroupSubjects
            .AsNoTracking()
            .Where(gs => gs.GroupId == groupId)
            .OrderBy(gs => gs.Id)
            .Select(gs => new SubjectResponse(gs.Subject!.Id, gs.Subject!.Name, gs.Subject!.UserId))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LessonResponse>> ListLessonsAsync(
        long studentId, long subjectId, CancellationToken ct = default)
    {
        if (!await access.HasSubjectAccessAsync(studentId, subjectId, ct))
        {
            throw new ForbiddenException("You do not have access to this subject");
        }

        var lessons = await db.Lessons
            .AsNoTracking()
            .Where(x => x.SubjectId == subjectId && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        return lessons
            .Select(l => new LessonResponse(l.Id, l.SubjectId, l.Title, storage.GetUrl(l.Image), l.SortOrder, l.IsActive))
            .ToList();
    }

    public async Task<IReadOnlyList<LessonContentResponse>> ListContentsAsync(
        long studentId, long lessonId, CancellationToken ct = default)
    {
        var lesson = await db.Lessons.AsNoTracking().FirstOrDefaultAsync(x => x.Id == lessonId, ct)
            ?? throw new KeyNotFoundException($"Lesson not found: {lessonId}");

        if (!await access.HasSubjectAccessAsync(studentId, lesson.SubjectId, ct))
        {
            throw new ForbiddenException("You do not have access to this lesson");
        }

        var contents = await db.LessonContents
            .AsNoTracking()
            .Where(x => x.LessonId == lessonId && x.IsActive)
            .Include(x => x.Video)
            .Include(x => x.MediaFile)
            .Include(x => x.VoiceRecord)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        return contents.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<QuizResponse>> ListQuizzesAsync(long studentId, CancellationToken ct = default)
    {
        return await (from gq in db.GroupQuizzes.AsNoTracking()
                      join q in db.Quizzes.AsNoTracking() on gq.QuizId equals q.Id
                      where db.StudentGroups.Any(sg => sg.GroupId == gq.GroupId && sg.StudentId == studentId)
                      orderby gq.Id
                      select new QuizResponse(
                          q.Id, q.LessonId, q.Title, q.DurationMinutes, q.Type, q.IsActive, q.IsRepeat, q.SortOrder,
                          db.QuizQuestions.Count(qq => qq.QuizId == q.Id)))
            .ToListAsync(ct);
    }

    private LessonContentResponse ToResponse(LessonContent content)
    {
        if (content.Video is not null)
            return Row(content, "video", storage.GetUrl(content.Video.Path), content.Video.DurationSeconds);
        if (content.MediaFile is not null)
            return Row(content, "file", storage.GetUrl(content.MediaFile.Path), null);
        if (content.VoiceRecord is not null)
            return Row(content, "voice", storage.GetUrl(content.VoiceRecord.Path), content.VoiceRecord.DurationSeconds);
        return Row(content, null, null, null);
    }

    private static LessonContentResponse Row(LessonContent c, string? type, string? url, int? duration) =>
        new(c.Id, c.LessonId, c.Title, c.Description, type, url, c.SortOrder, c.IsActive, duration);
}
