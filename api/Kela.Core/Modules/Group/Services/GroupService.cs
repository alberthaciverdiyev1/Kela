using Kela.Core.Common.Sorting;
using Kela.Core.Common.Pagination;
using Kela.Core.Common.Responses;
using Kela.Core.Common.Storage;
using Kela.Core.Common.Bulk;
using Kela.Core.Common.Exceptions;
using Kela.Core.Data;
using Kela.Core.Modules.Group.Requests;
using Kela.Core.Modules.Group.Responses;
using Kela.Core.Modules.Quiz.Responses;
using Kela.Core.Modules.Subject.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Group.Services;

using Group = Kela.Core.Modules.Group.Entities.Group;
using GroupQuiz = Kela.Core.Modules.Group.Entities.GroupQuiz;
using MainCategoryAssignmentLog = Kela.Core.Modules.Group.Entities.MainCategoryAssignmentLog;
using GroupSubject = Kela.Core.Modules.Group.Entities.GroupSubject;
using StudentGroup = Kela.Core.Modules.Group.Entities.StudentGroup;
using Subject = Kela.Core.Modules.Subject.Entities.Subject;

public sealed class GroupService(AppDbContext db, IFileStorage storage, ITenantFolder folders) : IGroupService
{
    private async Task<string> FolderAsync(CancellationToken ct)
        => $"{await folders.ResolveAsync(ct: ct)}/groups";

    public async Task<PagedResponse<GroupResponse>> ListAsync(GroupListRequest request, CancellationToken ct = default)
    {
        var (page, perPage) = Paging.Normalize(request);

        var query = db.Groups.AsNoTracking();

        if (request.IsDeleted == 1)
        {
            query = query.IgnoreQueryFilters(new[] { "SoftDelete" }).Where(x => x.DeletedAt != null);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == request.UserId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{search}%"));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .Include(x => x.Memberships)
            .ApplyListSort(request, x => x.Name)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(ct);

        var ids = items.Select(x => x.Id).ToList();
        var pending = await db.GroupJoinRequests.AsNoTracking()
            .Where(x => ids.Contains(x.GroupId) && x.Status == "pending")
            .GroupBy(x => x.GroupId)
            .Select(g => new { GroupId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.GroupId, x => x.Count, ct);

        var data = items.Select(x => ToResponse(x, pending.GetValueOrDefault(x.Id))).ToList();
        return new PagedResponse<GroupResponse>(data, page, Paging.LastPage(total, perPage), perPage, total);
    }

    public async Task<GroupResponse> GetAsync(long id, long? ownerId = null, CancellationToken ct = default)
    {
        var group = await db.Groups
            .IgnoreQueryFilters(new[] { "SoftDelete" })
            .AsNoTracking()
            .Include(x => x.Memberships)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Group not found: {id}");

        EnsureAccessible(group, ownerId);

        var pending = await db.GroupJoinRequests.AsNoTracking()
            .CountAsync(x => x.GroupId == group.Id && x.Status == "pending", ct);

        return ToResponse(group, pending);
    }

    public async Task<GroupResponse> CreateAsync(GroupUpsertRequest request, CancellationToken ct = default)
    {
        var image = request.Image is not null
            ? await storage.UploadAsync(request.Image, await FolderAsync(ct), ct)
            : null;

        var group = new Group
        {
            Name = request.Name,
            Description = request.Description,
            Image = image,
            UserId = request.UserId,
            JoinToken = Guid.NewGuid().ToString("N")
        };

        db.Groups.Add(group);
        await db.SaveChangesAsync(ct);

        return ToResponse(group);
    }

    public async Task<GroupResponse> UpdateAsync(long id, GroupUpsertRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        var group = await db.Groups
            .IgnoreQueryFilters(new[] { "SoftDelete" })
            .Include(x => x.Memberships)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Group not found: {id}");

        EnsureOwned(group, ownerId);

        if (request.RemoveImage)
        {
            await storage.DeleteAsync(group.Image, ct);
            group.Image = null;
        }
        else if (request.Image is not null)
        {
            await storage.DeleteAsync(group.Image, ct);
            group.Image = await storage.UploadAsync(request.Image, await FolderAsync(ct), ct);
        }

        group.Name = request.Name;
        group.Description = request.Description;
        // Sahiblik yalnız admin tərəfindən dəyişdirilə bilər (teacher öz sahibliyini itirməsin).
        if (ownerId is null) group.UserId = request.UserId;
        await db.SaveChangesAsync(ct);

        return ToResponse(group);
    }

    public async Task DeleteAsync(long id, long? ownerId = null, CancellationToken ct = default)
    {
        var group = await db.Groups.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Group not found: {id}");

        EnsureOwned(group, ownerId);

        group.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<GroupMemberResponse>> ListStudentsAsync(long groupId, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureGroupAccessibleAsync(groupId, ownerId, ct);
        return await MembersQuery(groupId).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<GroupMemberResponse>> AssignStudentsAsync(
        long groupId, AssignStudentsRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureGroupOwnedAsync(groupId, ownerId, ct);

        foreach (var studentId in request.StudentIds.Distinct())
        {
            var studentExists = await db.Students.AnyAsync(x => x.Id == studentId, ct);
            if (!studentExists)
            {
                throw new ArgumentException($"Student not found: {studentId}");
            }

            var existing = await db.StudentGroups
                .IgnoreQueryFilters(new[] { "SoftDelete" })
                .FirstOrDefaultAsync(x => x.GroupId == groupId && x.StudentId == studentId, ct);

            if (existing is null)
            {
                db.StudentGroups.Add(new StudentGroup
                {
                    GroupId = groupId,
                    StudentId = studentId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                });
            }
            else
            {
                existing.DeletedAt = null;
                existing.StartDate = request.StartDate;
                existing.EndDate = request.EndDate;
            }
        }

        await db.SaveChangesAsync(ct);
        return await MembersQuery(groupId).ToListAsync(ct);
    }

    public async Task RemoveStudentAsync(long groupId, long studentId, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureGroupOwnedAsync(groupId, ownerId, ct);

        var membership = await db.StudentGroups
            .FirstOrDefaultAsync(x => x.GroupId == groupId && x.StudentId == studentId, ct)
            ?? throw new KeyNotFoundException($"Membership not found for student {studentId} in group {groupId}");

        membership.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SubjectResponse>> ListSubjectsAsync(long groupId, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureGroupAccessibleAsync(groupId, ownerId, ct);

        return await db.GroupSubjects
            .AsNoTracking()
            .Where(x => x.GroupId == groupId)
            .OrderBy(x => x.Id)
            .Select(x => new SubjectResponse(x.Subject!.Id, x.Subject!.Name, x.Subject!.UserId, x.Subject!.IsShared))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SubjectResponse>> AssignSubjectsAsync(
        long groupId, AssignSubjectsRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureGroupOwnedAsync(groupId, ownerId, ct);

        foreach (var subjectId in request.SubjectIds.Distinct())
        {
            var subject = await db.Subjects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == subjectId, ct)
                ?? throw new ArgumentException($"Subject not found: {subjectId}");

            // Teacher yalnız öz və ya adminin fənnini qrupa bağlaya bilər.
            if (ownerId.HasValue && subject.UserId != ownerId && subject.UserId != null)
            {
                throw new ForbiddenException($"You do not have access to subject {subjectId}");
            }

            var existing = await db.GroupSubjects
                .IgnoreQueryFilters(new[] { "SoftDelete" })
                .FirstOrDefaultAsync(x => x.GroupId == groupId && x.SubjectId == subjectId, ct);

            if (existing is null)
            {
                db.GroupSubjects.Add(new GroupSubject { GroupId = groupId, SubjectId = subjectId });
            }
            else
            {
                existing.DeletedAt = null;
            }

            db.MainCategoryAssignmentLogs.Add(new MainCategoryAssignmentLog { GroupId = groupId, SubjectId = subjectId, Action = "attach" });
        }

        await db.SaveChangesAsync(ct);
        return await ListSubjectsAsync(groupId, ownerId, ct);
    }

    public async Task RemoveSubjectAsync(long groupId, long subjectId, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureGroupOwnedAsync(groupId, ownerId, ct);

        var link = await db.GroupSubjects
            .FirstOrDefaultAsync(x => x.GroupId == groupId && x.SubjectId == subjectId, ct)
            ?? throw new KeyNotFoundException($"Subject {subjectId} is not assigned to group {groupId}");

        link.DeletedAt = DateTimeOffset.UtcNow;
        db.MainCategoryAssignmentLogs.Add(new MainCategoryAssignmentLog { GroupId = groupId, SubjectId = subjectId, Action = "detach" });
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<QuizResponse>> ListQuizzesAsync(long groupId, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureGroupAccessibleAsync(groupId, ownerId, ct);

        return await (from gq in db.GroupQuizzes.AsNoTracking()
                      join q in db.Quizzes.AsNoTracking() on gq.QuizId equals q.Id
                      where gq.GroupId == groupId
                      orderby gq.Id
                      select new QuizResponse(
                          q.Id, q.LessonId, q.Title, q.DurationMinutes, q.Type, q.IsActive, q.IsRepeat, q.SortOrder,
                          db.QuizQuestions.Count(qq => qq.QuizId == q.Id)))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<QuizResponse>> AssignQuizzesAsync(
        long groupId, AssignQuizzesRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureGroupOwnedAsync(groupId, ownerId, ct);

        foreach (var quizId in request.QuizIds.Distinct())
        {
            var quiz = await db.Quizzes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == quizId, ct)
                ?? throw new ArgumentException($"Quiz not found: {quizId}");

            // Teacher yalnız öz və ya adminin imtahanını qrupa bağlaya bilər.
            if (ownerId.HasValue && quiz.UserId != ownerId && quiz.UserId != null)
            {
                throw new ForbiddenException($"You do not have access to quiz {quizId}");
            }

            var existing = await db.GroupQuizzes
                .IgnoreQueryFilters(new[] { "SoftDelete" })
                .FirstOrDefaultAsync(x => x.GroupId == groupId && x.QuizId == quizId, ct);

            if (existing is null)
            {
                db.GroupQuizzes.Add(new GroupQuiz { GroupId = groupId, QuizId = quizId });
            }
            else
            {
                existing.DeletedAt = null;
            }
        }

        await db.SaveChangesAsync(ct);
        return await ListQuizzesAsync(groupId, ownerId, ct);
    }

    public async Task RemoveQuizAsync(long groupId, long quizId, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureGroupOwnedAsync(groupId, ownerId, ct);

        var link = await db.GroupQuizzes
            .FirstOrDefaultAsync(x => x.GroupId == groupId && x.QuizId == quizId, ct)
            ?? throw new KeyNotFoundException($"Quiz {quizId} is not assigned to group {groupId}");

        link.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<GroupJoinRequestResponse>> ListJoinRequestsAsync(
        long groupId, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureGroupOwnedAsync(groupId, ownerId, ct);

        return await db.GroupJoinRequests.AsNoTracking()
            .Where(x => x.GroupId == groupId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new GroupJoinRequestResponse(
                x.Id,
                x.StudentId,
                x.Student!.FullName,
                x.Student!.Email,
                x.Status,
                x.CreatedAt,
                x.DecidedAt))
            .ToListAsync(ct);
    }

    public async Task DecideJoinRequestAsync(
        long groupId, long requestId, bool approve, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureGroupOwnedAsync(groupId, ownerId, ct);

        var request = await db.GroupJoinRequests
            .FirstOrDefaultAsync(x => x.Id == requestId && x.GroupId == groupId, ct)
            ?? throw new KeyNotFoundException($"Join request not found: {requestId}");

        if (request.Status != "pending")
        {
            throw new ArgumentException("This join request has already been decided");
        }

        if (approve)
        {
            var exists = await db.StudentGroups
                .IgnoreQueryFilters(new[] { "SoftDelete" })
                .FirstOrDefaultAsync(x => x.GroupId == groupId && x.StudentId == request.StudentId, ct);

            if (exists is null)
            {
                db.StudentGroups.Add(new StudentGroup { GroupId = groupId, StudentId = request.StudentId });
            }
            else
            {
                exists.DeletedAt = null;
            }
        }

        request.Status = approve ? "approved" : "rejected";
        request.DecidedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<int> CountPendingJoinRequestsAsync(long? ownerId = null, CancellationToken ct = default)
    {
        var query = db.GroupJoinRequests.AsNoTracking().Where(x => x.Status == "pending");

        if (ownerId.HasValue)
        {
            query = query.Where(x => db.Groups.Any(g => g.Id == x.GroupId && g.UserId == ownerId));
        }

        return await query.CountAsync(ct);
    }

    private async Task EnsureGroupAccessibleAsync(long groupId, long? ownerId, CancellationToken ct)
    {
        if (!ownerId.HasValue)
        {
            await EnsureGroupExistsAsync(groupId, ct);
            return;
        }

        var owner = await db.Groups.AsNoTracking().Where(x => x.Id == groupId)
            .Select(x => (long?)x.UserId).FirstOrDefaultAsync(ct);

        if (owner != ownerId && owner != null)
        {
            throw new ForbiddenException("You do not have access to this group");
        }
    }

    private async Task EnsureGroupOwnedAsync(long groupId, long? ownerId, CancellationToken ct)
    {
        if (!ownerId.HasValue)
        {
            await EnsureGroupExistsAsync(groupId, ct);
            return;
        }

        var owner = await db.Groups.AsNoTracking().Where(x => x.Id == groupId)
            .Select(x => (long?)x.UserId).FirstOrDefaultAsync(ct);

        if (owner != ownerId)
        {
            throw new ForbiddenException("You can only modify your own group");
        }
    }

    private static void EnsureAccessible(Group group, long? ownerId)
    {
        if (ownerId.HasValue && group.UserId != ownerId && group.UserId != null)
        {
            throw new ForbiddenException("You do not have access to this group");
        }
    }

    private static void EnsureOwned(Group group, long? ownerId)
    {
        if (ownerId.HasValue && group.UserId != ownerId)
        {
            throw new ForbiddenException("You can only modify your own group");
        }
    }

    private async Task EnsureGroupExistsAsync(long groupId, CancellationToken ct)
    {
        if (!await db.Groups.AnyAsync(x => x.Id == groupId, ct))
        {
            throw new KeyNotFoundException($"Group not found: {groupId}");
        }
    }

    private IQueryable<GroupMemberResponse> MembersQuery(long groupId) =>
        db.StudentGroups
            .AsNoTracking()
            .Where(x => x.GroupId == groupId)
            .OrderBy(x => x.Id)
            .Select(x => new GroupMemberResponse(
                x.StudentId,
                x.Student!.FullName,
                x.Student!.Email,
                x.StartDate,
                x.EndDate));

    private GroupResponse ToResponse(Group group, int pendingJoins = 0) =>
        new(
            group.Id,
            group.Name,
            group.Description,
            storage.GetUrl(group.Image),
            group.UserId,
            group.Memberships.Count,
            group.JoinToken,
            pendingJoins);

    public Task BulkAsync(BulkRequest request, long? ownerId = null, CancellationToken ct = default)
        => db.BulkOwnedAsync<Group>(request, ownerId, x => x.UserId, "You can only modify your own groups", ct);

    public async Task<IReadOnlyList<AssignmentLogResponse>> ListSubjectLogsAsync(long groupId, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureGroupAccessibleAsync(groupId, ownerId, ct);
        return await db.MainCategoryAssignmentLogs.AsNoTracking()
            .Where(x => x.GroupId == groupId)
            .OrderByDescending(x => x.Id)
            .Select(x => new AssignmentLogResponse(x.Id, x.SubjectId, x.GroupId, x.Action, x.CreatedAt))
            .ToListAsync(ct);
    }
}
