using System.Text.Json;
using Kela.Core.Common.Exceptions;
using Kela.Core.Common.Storage;
using Kela.Core.Data;
using Kela.Core.Modules.Group.Responses;
using Kela.Core.Modules.Lesson.Responses;
using Kela.Core.Modules.LessonContent.Responses;
using Kela.Core.Modules.Quiz.Responses;
using Kela.Core.Modules.StudentApp.Responses;
using Kela.Core.Modules.Subject.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.StudentApp.Services;

using LessonContent = Kela.Core.Modules.LessonContent.Entities.LessonContent;
using GroupJoinRequest = Kela.Core.Modules.Group.Entities.GroupJoinRequest;

public sealed class StudentAppService(
    AppDbContext db,
    IFileStorage storage,
    IStudentAccessService access) : IStudentAppService
{
    public async Task<IReadOnlyList<StudentGroupResponse>> ListGroupsAsync(long studentId, CancellationToken ct = default)
    {
        var groups = await db.Groups
            .AsNoTracking()
            .Where(g => db.StudentGroups.Any(sg => sg.GroupId == g.Id && sg.StudentId == studentId))
            .Include(g => g.Memberships)
            .OrderByDescending(g => g.Id)
            .ToListAsync(ct);

        // Şagirdin progressi qruplara görə backend-də hesablanır.
        var accessibleSubjectIds = (await access.AccessibleSubjectIdsAsync(studentId, ct)).ToHashSet();
        var groupIds = groups.Select(g => g.Id).ToList();

        var groupSubjects = await db.GroupSubjects.AsNoTracking()
            .Where(gs => groupIds.Contains(gs.GroupId) && accessibleSubjectIds.Contains(gs.SubjectId))
            .Select(gs => new { gs.GroupId, gs.SubjectId })
            .ToListAsync(ct);

        var subjectIds = groupSubjects.Select(x => x.SubjectId).Distinct().ToList();

        var lessons = await db.Lessons.AsNoTracking()
            .Where(l => subjectIds.Contains(l.SubjectId) && l.IsActive)
            .Select(l => new { l.Id, l.SubjectId })
            .ToListAsync(ct);

        var lessonIds = lessons.Select(l => l.Id).ToList();

        var contents = await db.LessonContents.AsNoTracking()
            .Where(c => lessonIds.Contains(c.LessonId) && c.IsActive)
            .Select(c => new { c.Id, c.LessonId })
            .ToListAsync(ct);

        var completed = (await db.LessonProgress
            .Where(p => p.StudentId == studentId)
            .Select(p => p.LessonContentId)
            .ToListAsync(ct)).ToHashSet();

        return groups.Select(g =>
        {
            var subjectIdsOfGroup = groupSubjects.Where(x => x.GroupId == g.Id).Select(x => x.SubjectId).ToHashSet();
            var lessonIdsOfGroup = lessons.Where(l => subjectIdsOfGroup.Contains(l.SubjectId)).Select(l => l.Id).ToHashSet();
            var contentIdsOfGroup = contents.Where(c => lessonIdsOfGroup.Contains(c.LessonId)).Select(c => c.Id).ToList();
            var done = contentIdsOfGroup.Count(completed.Contains);

            return new StudentGroupResponse(
                g.Id,
                g.Name,
                g.Description,
                storage.GetUrl(g.Image),
                g.Memberships.Count,
                contentIdsOfGroup.Count,
                done,
                Percent(done, contentIdsOfGroup.Count));
        }).ToList();
    }

    public async Task<GroupJoinInfoResponse> GetGroupJoinInfoAsync(
        long studentId, string token, CancellationToken ct = default)
    {
        var group = await db.Groups.AsNoTracking().FirstOrDefaultAsync(x => x.JoinToken == token, ct)
            ?? throw new KeyNotFoundException("Group not found");

        return await BuildJoinInfoAsync(studentId, group, ct);
    }

    public async Task<GroupJoinInfoResponse> GetPublicGroupJoinInfoAsync(string token, CancellationToken ct = default)
    {
        // Giriş etməmiş istifadəçi üçün tenant filtri əl ilə söndürülür — token unikaldır.
        var group = await db.Groups.IgnoreQueryFilters(new[] { "Tenant" }).AsNoTracking()
            .FirstOrDefaultAsync(x => x.JoinToken == token && x.DeletedAt == null, ct)
            ?? throw new KeyNotFoundException("Group not found");

        string? teacherName = null;
        if (group.UserId is long teacherId)
        {
            var teacher = await db.Users.IgnoreQueryFilters(new[] { "Tenant" }).AsNoTracking()
                .Where(x => x.Id == teacherId)
                .Select(x => new { x.Name, x.Surname })
                .FirstOrDefaultAsync(ct);

            teacherName = teacher is null ? null : $"{teacher.Name} {teacher.Surname}".Trim();
        }

        return new GroupJoinInfoResponse(
            group.Id,
            group.Name,
            group.Description,
            string.IsNullOrWhiteSpace(group.Image) ? null : storage.GetUrl(group.Image),
            teacherName,
            "anonymous",
            group.TenantId);
    }

    public async Task<GroupJoinInfoResponse> RequestJoinGroupAsync(
        long studentId, string token, CancellationToken ct = default)
    {
        var group = await db.Groups.FirstOrDefaultAsync(x => x.JoinToken == token, ct)
            ?? throw new KeyNotFoundException("Group not found");

        var isMember = await db.StudentGroups
            .AnyAsync(x => x.GroupId == group.Id && x.StudentId == studentId, ct);

        if (!isMember)
        {
            var request = await db.GroupJoinRequests.IgnoreQueryFilters(new[] { "SoftDelete" })
                .FirstOrDefaultAsync(x => x.GroupId == group.Id && x.StudentId == studentId, ct);

            if (request is null)
            {
                db.GroupJoinRequests.Add(new GroupJoinRequest
                {
                    GroupId = group.Id,
                    StudentId = studentId,
                    Status = "pending"
                });
            }
            else if (request.Status == "rejected")
            {
                // Yenidən müraciət: rədd edilmiş sorğu yenidən gözləməyə keçir.
                request.Status = "pending";
                request.DecidedAt = null;
                request.DeletedAt = null;
            }

            await db.SaveChangesAsync(ct);
        }

        return await BuildJoinInfoAsync(studentId, group, ct);
    }

    private async Task<GroupJoinInfoResponse> BuildJoinInfoAsync(
        long studentId, Kela.Core.Modules.Group.Entities.Group group, CancellationToken ct)
    {
        var isMember = await db.StudentGroups
            .AnyAsync(x => x.GroupId == group.Id && x.StudentId == studentId, ct);

        var request = await db.GroupJoinRequests.AsNoTracking()
            .Where(x => x.GroupId == group.Id && x.StudentId == studentId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        var status = isMember ? "member" : request?.Status ?? "none";

        string? teacherName = null;
        if (group.UserId is long teacherId)
        {
            var teacher = await db.Users.AsNoTracking()
                .Where(x => x.Id == teacherId)
                .Select(x => new { x.Name, x.Surname })
                .FirstOrDefaultAsync(ct);

            teacherName = teacher is null ? null : $"{teacher.Name} {teacher.Surname}".Trim();
        }

        return new GroupJoinInfoResponse(
            group.Id,
            group.Name,
            group.Description,
            string.IsNullOrWhiteSpace(group.Image) ? null : storage.GetUrl(group.Image),
            teacherName,
            status,
            group.TenantId);
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

    public async Task<SubjectResponse> GetSubjectAsync(long studentId, long subjectId, CancellationToken ct = default)
    {
        if (!await access.HasSubjectAccessAsync(studentId, subjectId, ct))
        {
            throw new ForbiddenException("You do not have access to this subject");
        }

        var subject = await db.Subjects.AsNoTracking().FirstOrDefaultAsync(s => s.Id == subjectId, ct)
            ?? throw new KeyNotFoundException($"Subject not found: {subjectId}");

        return new SubjectResponse(subject.Id, subject.Name, subject.UserId);
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
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .ToListAsync(ct);

        lessons = await ApplyWindowsAsync(studentId, subjectId, lessons, l => l.CreatedAt, ct);

        return lessons
            .Select(l => new LessonResponse(l.Id, l.SubjectId, l.Title, storage.GetUrl(l.Image), l.SortOrder, l.IsActive, l.Settings))
            .ToList();
    }

    public async Task<LessonResponse> GetLessonAsync(long studentId, long lessonId, CancellationToken ct = default)
    {
        var lesson = await db.Lessons.AsNoTracking().FirstOrDefaultAsync(x => x.Id == lessonId, ct)
            ?? throw new KeyNotFoundException($"Lesson not found: {lessonId}");

        if (!await access.HasSubjectAccessAsync(studentId, lesson.SubjectId, ct))
        {
            throw new ForbiddenException("You do not have access to this lesson");
        }

        return new LessonResponse(lesson.Id, lesson.SubjectId, lesson.Title, storage.GetUrl(lesson.Image),
            lesson.SortOrder, lesson.IsActive, lesson.Settings);
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
            .Where(x => x.IsActive
                        && (x.LessonId == lessonId || x.AdditionalLessons.Any(l => l.LessonId == lessonId)))
            .Include(x => x.Video).Include(x => x.MediaFile).Include(x => x.VoiceRecord)
            .Include(x => x.AdditionalLessons)
            .ToListAsync(ct);

        contents = await ApplyWindowsAsync(studentId, lesson.SubjectId, contents, c => c.CreatedAt, ct);

        // Əsas dərsdə materialın öz sırası, paylaşıldığı dərslərdə isə həmin dərs üçün sıra.
        var ordered = contents
            .OrderBy(c => c.LessonId == lessonId
                ? c.SortOrder
                : c.AdditionalLessons.FirstOrDefault(l => l.LessonId == lessonId)?.SortOrder ?? c.SortOrder)
            .ThenBy(c => c.Id)
            .ToList();

        return ordered.Select(ToResponse).ToList();
    }

    public async Task<ContentCountResponse> GetContentCountAsync(long studentId, long lessonId, CancellationToken ct = default)
    {
        var lesson = await db.Lessons.AsNoTracking().FirstOrDefaultAsync(x => x.Id == lessonId, ct)
            ?? throw new KeyNotFoundException($"Lesson not found: {lessonId}");

        if (!await access.HasSubjectAccessAsync(studentId, lesson.SubjectId, ct))
        {
            throw new ForbiddenException("You do not have access to this lesson");
        }

        return ParseCounts(lesson.Settings);
    }

    public async Task<IReadOnlyList<SubjectResponse>> ListAllSubjectsAsync(long studentId, CancellationToken ct = default)
    {
        var subjectIds = await db.StudentGroups.AsNoTracking()
            .Where(sg => sg.StudentId == studentId)
            .Join(db.GroupSubjects.AsNoTracking(), sg => sg.GroupId, gs => gs.GroupId, (_, gs) => gs.SubjectId)
            .Distinct()
            .ToListAsync(ct);

        return await db.Subjects.AsNoTracking()
            .Where(s => subjectIds.Contains(s.Id))
            .OrderBy(s => s.Name)
            .Select(s => new SubjectResponse(s.Id, s.Name, s.UserId))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<QuizResponse>> ListQuizzesAsync(
        long studentId, long? subjectId = null, long? lessonId = null, bool general = false, CancellationToken ct = default)
    {
        var quizzes = db.GroupQuizzes.AsNoTracking()
            .Where(gq => db.StudentGroups.Any(sg => sg.GroupId == gq.GroupId && sg.StudentId == studentId))
            .Join(db.Quizzes.AsNoTracking(), gq => gq.QuizId, q => q.Id, (_, q) => q);

        if (lessonId is long lid)
        {
            quizzes = quizzes.Where(q => q.LessonId == lid);
        }

        if (general)
        {
            quizzes = quizzes.Where(q => q.LessonId == null);
        }

        if (subjectId is long sid)
        {
            quizzes = quizzes.Where(q => db.Lessons.Any(l => l.Id == q.LessonId && l.SubjectId == sid));
        }

        return await quizzes.OrderBy(q => q.Id)
            .Select(q => new QuizResponse(
                q.Id, q.LessonId, q.Title, q.DurationMinutes, q.Type, q.IsActive, q.IsRepeat, q.SortOrder,
                db.QuizQuestions.Count(qq => qq.QuizId == q.Id),
                db.Lessons.Where(l => l.Id == q.LessonId).Select(l => l.Title).FirstOrDefault()))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<QuizResponse>> ListLessonQuizzesAsync(long studentId, long lessonId, CancellationToken ct = default)
    {
        return await (from gq in db.GroupQuizzes.AsNoTracking()
                      join q in db.Quizzes.AsNoTracking() on gq.QuizId equals q.Id
                      where q.LessonId == lessonId
                            && db.StudentGroups.Any(sg => sg.GroupId == gq.GroupId && sg.StudentId == studentId)
                      orderby gq.Id
                      select new QuizResponse(
                          q.Id, q.LessonId, q.Title, q.DurationMinutes, q.Type, q.IsActive, q.IsRepeat, q.SortOrder,
                          db.QuizQuestions.Count(qq => qq.QuizId == q.Id)))
            .ToListAsync(ct);
    }

    /// <summary>Şagirdin tarixli üzvülük pəncərələri varsa, yalnız həmin aralıqlarda yaradılan qeydlər qalır.</summary>
    private async Task<List<T>> ApplyWindowsAsync<T>(
        long studentId, long subjectId, List<T> items, Func<T, DateTimeOffset> createdAt, CancellationToken ct)
    {
        var windows = await access.MembershipWindowsAsync(studentId, subjectId, ct);
        if (windows.Count == 0) return items;

        return items.Where(item =>
        {
            var date = DateOnly.FromDateTime(createdAt(item).UtcDateTime);
            return windows.Any(w => date >= w.Start && (w.End is null || date <= w.End));
        }).ToList();
    }

    private static ContentCountResponse ParseCounts(string settings)
    {
        try
        {
            using var doc = JsonDocument.Parse(settings);
            var root = doc.RootElement;
            return new ContentCountResponse(
                ReadFlag(root, "topic_explain"),
                ReadFlag(root, "live_video"),
                ReadFlag(root, "pdf"),
                ReadFlag(root, "voice_record"),
                ReadFlag(root, "topic_quiz"),
                ReadFlag(root, "general_quiz"));
        }
        catch (JsonException)
        {
            return new ContentCountResponse(1, 1, 1, 1, 1, 1);
        }
    }

    private static int ReadFlag(JsonElement root, string name)
        => root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number ? value.GetInt32() : 1;

    private static int Percent(int part, int total) => total > 0 ? (int)Math.Round(part * 100.0 / total) : 0;

    private LessonContentResponse ToResponse(LessonContent content)
    {
        if (content.Video is not null)
            return Row(content, "video", storage.GetUrl(content.Video.Path), ResolvePlayback(content.Video.PlaybackUrl),
                content.Video.DurationSeconds, ThumbnailUrl(content.Video.Thumbnail), content.Video.ConversionStatus);
        if (content.MediaFile is not null)
            return Row(content, "file", storage.GetUrl(content.MediaFile.Path), null, null, null);
        if (content.VoiceRecord is not null)
            return Row(content, "voice", storage.GetUrl(content.VoiceRecord.Path), null, content.VoiceRecord.DurationSeconds, null);
        return Row(content, null, null, null, null, null);
    }

    /// <summary>
    /// Worker m3u8 açarını (nisbi yol) bildirə bilər — o halda URL-ə çeviririk.
    /// Artıq tam URL-dirsə storage onu olduğu kimi qaytarır.
    /// </summary>
    private string? ResolvePlayback(string? playbackUrl)
        => string.IsNullOrWhiteSpace(playbackUrl) ? null : storage.GetUrl(playbackUrl);

    private string? ThumbnailUrl(string? thumbnail)
        => string.IsNullOrWhiteSpace(thumbnail) ? null : storage.GetUrl(thumbnail);

    private static LessonContentResponse Row(LessonContent c, string? type, string? url, string? playbackUrl,
        int? duration, string? thumbnailUrl, string? conversionStatus = null) =>
        new(c.Id, c.LessonId, c.Title, c.Description, type, url, playbackUrl, c.SortOrder, c.IsActive,
            duration, thumbnailUrl, conversionStatus);
}
