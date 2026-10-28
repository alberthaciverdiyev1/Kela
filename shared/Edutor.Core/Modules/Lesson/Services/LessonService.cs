using System.Text.Json;
using Edutor.Core.Common.Pagination;
using Edutor.Core.Common.Responses;
using Edutor.Core.Common.Storage;
using Edutor.Core.Common.Bulk;
using Edutor.Core.Common.Exceptions;
using Edutor.Core.Common.Sorting;
using Edutor.Core.Data;
using Edutor.Core.Modules.Lesson.Entities;
using Edutor.Core.Modules.Lesson.Requests;
using Edutor.Core.Modules.Lesson.Responses;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.Lesson.Services;
using GroupLesson = Edutor.Core.Modules.Group.Entities.GroupLesson;

using Lesson = Edutor.Core.Modules.Lesson.Entities.Lesson;
using Edutor.Core.Common.Queries;
using Edutor.Core.Common.Authorization;

public sealed class LessonService(
    AppDbContext db,
    IFileStorage storage,
    ITenantFolder folders,
    Edutor.Core.Modules.Subscription.Abstractions.ISubscriptionService subscription) : ILessonService
{
    private async Task<string> FolderAsync(CancellationToken ct)
        => $"{await folders.ResolveAsync(ct: ct)}/lessons";

    public async Task<PagedResponse<LessonResponse>> ListAsync(LessonListRequest request, CancellationToken ct = default)
    {

        var query = db.Lessons.AsNoTracking();

        query = query.ApplyTrashFilter(request.IsDeleted);

        if (request.SubjectId.HasValue)
        {
            query = query.Where(x => x.SubjectId == request.SubjectId);
        }

        if (request.UserId is long ownerId)
        {
            // Teacher öz dərslərini, paylaşılan dərsləri və paylaşılan fənnin
            // bütün dərslərini görür.
            query = query.Where(x => x.UserId == ownerId || x.IsShared || x.Subject!.IsShared);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive);
        }

        query = query.SearchBy(request.Search, x => x.Title);

        return await query
            .ApplyListSort(request, x => x.Title, x => x.SortOrder)
        .ToPagedAsync(request, ToResponse, ct);
    }

    public async Task<LessonResponse> GetAsync(long id, long? ownerId = null, CancellationToken ct = default)
    {
        var lesson = await db.Lessons.IgnoreQueryFilters(new[] { "SoftDelete" }).AsNoTracking()
            .Include(x => x.Subject)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Lesson not found: {id}");

        OwnershipGuard.EnsureReadable(ownerId, lesson.UserId, IsShared(lesson), "You do not have access to this lesson");
        return ToResponse(lesson);
    }

    public async Task<LessonResponse> CreateAsync(LessonUpsertRequest request, CancellationToken ct = default)
    {
        await subscription.EnsureCanAddLessonsAsync(1, ct);
        await EnsureSubjectExistsAsync(request.SubjectId, ct);

        // Teacher yalnız öz və ya adminin fənninə dərs əlavə edə bilər.
        if (request.UserId is long ownerId)
        {
            var subject = await db.Subjects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.SubjectId, ct);
            if (subject is null || (subject.UserId != ownerId && !subject.IsShared))
            {
                throw new ForbiddenException("You do not have access to this subject");
            }
        }

        var image = request.Image is not null
            ? await storage.UploadAsync(request.Image, await FolderAsync(ct), ct)
            : null;

        var lesson = new Lesson
        {
            SubjectId = request.SubjectId,
            Title = request.Title,
            Image = image,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            IsShared = request.IsShared,
            UserId = request.UserId,
            Settings = string.IsNullOrWhiteSpace(request.Settings) ? LessonDefaults.Settings : request.Settings
        };

        db.Lessons.Add(lesson);
        await db.SaveChangesAsync(ct);

        if (request.AddToGroups)
        {
            await AddLessonToConfiguredGroupsAsync(lesson, ct);
        }

        return ToResponse(lesson);
    }

    /// <summary>
    /// Dərs seçimi edilmiş qruplara yeni dərsi əlavə edir. Seçim etməyən (köhnə)
    /// qruplar bütün dərsləri gördüyü üçün onlara sətir yazmaq lazım deyil.
    /// </summary>
    private async Task AddLessonToConfiguredGroupsAsync(Edutor.Core.Modules.Lesson.Entities.Lesson lesson, CancellationToken ct)
    {
        var groupIds = await db.GroupSubjects
            .Where(gs => gs.SubjectId == lesson.SubjectId && gs.LessonsConfigured)
            .Select(gs => gs.GroupId)
            .ToListAsync(ct);

        if (groupIds.Count == 0) return;

        var already = await db.GroupLessons
            .Where(gl => groupIds.Contains(gl.GroupId) && gl.LessonId == lesson.Id)
            .Select(gl => gl.GroupId)
            .ToListAsync(ct);

        foreach (var groupId in groupIds.Except(already))
        {
            db.GroupLessons.Add(new GroupLesson { GroupId = groupId, LessonId = lesson.Id });
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task<LessonResponse> UpdateAsync(long id, LessonUpsertRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        var lesson = await db.Lessons.IgnoreQueryFilters(new[] { "SoftDelete" })
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Lesson not found: {id}");

        OwnershipGuard.EnsureOwned(ownerId, lesson.UserId, "You can only modify your own lesson");
        await EnsureSubjectExistsAsync(request.SubjectId, ct);

        if (request.RemoveImage)
        {
            await storage.DeleteAsync(lesson.Image, ct);
            lesson.Image = null;
        }
        else if (request.Image is not null)
        {
            await storage.DeleteAsync(lesson.Image, ct);
            lesson.Image = await storage.UploadAsync(request.Image, await FolderAsync(ct), ct);
        }

        lesson.SubjectId = request.SubjectId;
        lesson.Title = request.Title;
        lesson.SortOrder = request.SortOrder;
        lesson.IsActive = request.IsActive;
        // Paylaşımı yalnız sahibi və ya admin dəyişir — yuxarıdaki EnsureOwned bunu təmin edir.
        lesson.IsShared = request.IsShared;
        if (!string.IsNullOrWhiteSpace(request.Settings)) lesson.Settings = request.Settings;
        await db.SaveChangesAsync(ct);

        return ToResponse(lesson);
    }

    public async Task DeleteAsync(long id, long? ownerId = null, CancellationToken ct = default)
    {
        var lesson = await db.Lessons.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Lesson not found: {id}");

        OwnershipGuard.EnsureOwned(ownerId, lesson.UserId, "You can only modify your own lesson");

        lesson.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task EnsureSubjectExistsAsync(long subjectId, CancellationToken ct)
    {
        if (!await db.Subjects.AnyAsync(x => x.Id == subjectId, ct))
        {
            throw new ArgumentException($"Subject not found: {subjectId}");
        }
    }

    /// <summary>Dərs digər müəllimlərə açıqdır — özü paylaşılıbsa və ya fənni paylaşılıbsa.</summary>
    private static bool IsShared(Lesson lesson) => lesson.IsShared || lesson.Subject?.IsShared == true;

    private LessonResponse ToResponse(Lesson lesson) =>
        new(
            lesson.Id,
            lesson.SubjectId,
            lesson.Title,
            storage.GetUrl(lesson.Image),
            lesson.SortOrder,
            lesson.IsActive,
            lesson.Settings,
            lesson.IsShared,
            lesson.UserId);

    /// <summary>
    /// Paylaşılan dərsin statusunu digər müəllim də dəyişə bilər — paylaşım
    /// "baxmaq" deyil, birlikdə işləmək deməkdir.
    /// </summary>
    public Task SetActiveAsync(long id, bool isActive, long? ownerId = null, CancellationToken ct = default)
        => db.SetActiveAsync<Lesson>(id, isActive, ownerId, x => x.UserId, "You can only modify your own lesson", ct,
            x => x.IsShared || (x.Subject != null && x.Subject.IsShared),
            q => q.Include(x => x.Subject));

    public Task BulkAsync(BulkRequest request, long? ownerId = null, CancellationToken ct = default)
        => db.BulkOwnedAsync<Lesson>(request, ownerId, x => x.UserId, "You can only modify your own lessons", ct);

    public Task ReorderAsync(SortRequest request, long? ownerId = null, CancellationToken ct = default)
        => db.ReorderOwnedAsync<Lesson>(request, ownerId, x => x.UserId, "You can only modify your own lessons", ct);

    public async Task<LessonSettingsResponse> GetSettingsAsync(long id, long? ownerId = null, CancellationToken ct = default)
    {
        var lesson = await db.Lessons.AsNoTracking().Include(x => x.Subject).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Lesson not found: {id}");

        OwnershipGuard.EnsureReadable(ownerId, lesson.UserId, IsShared(lesson), "You do not have access to this lesson");
        return LessonSettings.Parse(lesson.Settings);
    }

    public async Task<LessonSettingsResponse> SetSettingsAsync(long id, SetLessonSettingsRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        var lesson = await db.Lessons.IgnoreQueryFilters(new[] { "SoftDelete" }).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Lesson not found: {id}");

        OwnershipGuard.EnsureOwned(ownerId, lesson.UserId, "You can only modify your own lesson");
        lesson.Settings = LessonSettings.Serialize(request);
        await db.SaveChangesAsync(ct);
        return LessonSettings.Parse(lesson.Settings);
    }
}
