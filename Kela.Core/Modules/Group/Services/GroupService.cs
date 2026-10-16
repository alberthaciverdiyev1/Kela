using Microsoft.Extensions.DependencyInjection;
using Kela.Core.Modules.Students.Abstractions;
using Kela.Core.Modules.Students.Requests;
using Kela.Core.Common.MultiTenancy;
using Kela.Core.Common.Jobs;
using Kela.Core.Common.Sorting;
using Kela.Core.Common.Responses;
using Kela.Core.Common.Storage;
using Kela.Core.Common.Bulk;
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

public sealed class GroupService(AppDbContext db, IFileStorage storage, ITenantFolder folders, IBackgroundJobs jobs) : IGroupService
{
    private async Task<string> FolderAsync(CancellationToken ct)
        => $"{await folders.ResolveAsync(ct: ct)}/groups";

    public async Task<PagedResponse<GroupResponse>> ListAsync(GroupListRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var perPage = request.PerPage < 1 ? 20 : Math.Min(request.PerPage, 500);

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

        var data = items.Select(ToResponse).ToList();
        var lastPage = (int)Math.Ceiling(total / (double)perPage);

        return new PagedResponse<GroupResponse>(data, page, lastPage, perPage, total);
    }

    public async Task<GroupResponse> GetAsync(long id, CancellationToken ct = default)
    {
        var group = await db.Groups
            .IgnoreQueryFilters(new[] { "SoftDelete" })
            .AsNoTracking()
            .Include(x => x.Memberships)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Group not found: {id}");

        return ToResponse(group);
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
            UserId = request.UserId
        };

        db.Groups.Add(group);
        await db.SaveChangesAsync(ct);

        // Şagird generasiyası baha başa gəlir (BCrypt) — sorğunu gözlətməmək üçün
        // arxa plana atılır və nəticə birbaşa bu qrupa əlavə olunur.
        if (request.GenerateCount > 0)
        {
            var tenantId = group.TenantId;
            var groupId = group.Id;
            var spec = new GenerateStudentsRequest
            {
                Count = request.GenerateCount,
                GroupId = groupId,
                EmailPrefix = string.IsNullOrWhiteSpace(request.GeneratePrefix) ? "student" : request.GeneratePrefix,
                EmailDomain = string.IsNullOrWhiteSpace(request.GenerateDomain) ? "kela.az" : request.GenerateDomain,
                Password = request.GeneratePassword
            };

            jobs.Enqueue($"group-{groupId}-generate-students", async (sp, token) =>
            {
                // Arxa planda HTTP konteksti yoxdur — tenant açıq şəkildə verilir.
                sp.GetRequiredService<ITenantScope>().Override = tenantId;
                await sp.GetRequiredService<IStudentManagementService>().GenerateAsync(spec, token);
            });
        }

        return ToResponse(group);
    }

    public async Task<GroupResponse> UpdateAsync(long id, GroupUpsertRequest request, CancellationToken ct = default)
    {
        var group = await db.Groups
            .IgnoreQueryFilters(new[] { "SoftDelete" })
            .Include(x => x.Memberships)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Group not found: {id}");

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
        group.UserId = request.UserId;
        await db.SaveChangesAsync(ct);

        return ToResponse(group);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var group = await db.Groups.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Group not found: {id}");

        group.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<GroupMemberResponse>> ListStudentsAsync(long groupId, CancellationToken ct = default)
    {
        await EnsureGroupExistsAsync(groupId, ct);
        return await MembersQuery(groupId).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<GroupMemberResponse>> AssignStudentsAsync(
        long groupId, AssignStudentsRequest request, CancellationToken ct = default)
    {
        await EnsureGroupExistsAsync(groupId, ct);

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

    public async Task RemoveStudentAsync(long groupId, long studentId, CancellationToken ct = default)
    {
        var membership = await db.StudentGroups
            .FirstOrDefaultAsync(x => x.GroupId == groupId && x.StudentId == studentId, ct)
            ?? throw new KeyNotFoundException($"Membership not found for student {studentId} in group {groupId}");

        membership.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SubjectResponse>> ListSubjectsAsync(long groupId, CancellationToken ct = default)
    {
        await EnsureGroupExistsAsync(groupId, ct);

        return await db.GroupSubjects
            .AsNoTracking()
            .Where(x => x.GroupId == groupId)
            .OrderBy(x => x.Id)
            .Select(x => new SubjectResponse(x.Subject!.Id, x.Subject!.Name, x.Subject!.UserId))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SubjectResponse>> AssignSubjectsAsync(
        long groupId, AssignSubjectsRequest request, CancellationToken ct = default)
    {
        await EnsureGroupExistsAsync(groupId, ct);

        foreach (var subjectId in request.SubjectIds.Distinct())
        {
            if (!await db.Subjects.AnyAsync(x => x.Id == subjectId, ct))
            {
                throw new ArgumentException($"Subject not found: {subjectId}");
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
        return await ListSubjectsAsync(groupId, ct);
    }

    public async Task RemoveSubjectAsync(long groupId, long subjectId, CancellationToken ct = default)
    {
        var link = await db.GroupSubjects
            .FirstOrDefaultAsync(x => x.GroupId == groupId && x.SubjectId == subjectId, ct)
            ?? throw new KeyNotFoundException($"Subject {subjectId} is not assigned to group {groupId}");

        link.DeletedAt = DateTimeOffset.UtcNow;
        db.MainCategoryAssignmentLogs.Add(new MainCategoryAssignmentLog { GroupId = groupId, SubjectId = subjectId, Action = "detach" });
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<QuizResponse>> ListQuizzesAsync(long groupId, CancellationToken ct = default)
    {
        await EnsureGroupExistsAsync(groupId, ct);

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
        long groupId, AssignQuizzesRequest request, CancellationToken ct = default)
    {
        await EnsureGroupExistsAsync(groupId, ct);

        foreach (var quizId in request.QuizIds.Distinct())
        {
            if (!await db.Quizzes.AnyAsync(x => x.Id == quizId, ct))
            {
                throw new ArgumentException($"Quiz not found: {quizId}");
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
        return await ListQuizzesAsync(groupId, ct);
    }

    public async Task RemoveQuizAsync(long groupId, long quizId, CancellationToken ct = default)
    {
        var link = await db.GroupQuizzes
            .FirstOrDefaultAsync(x => x.GroupId == groupId && x.QuizId == quizId, ct)
            ?? throw new KeyNotFoundException($"Quiz {quizId} is not assigned to group {groupId}");

        link.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
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

    private GroupResponse ToResponse(Group group) =>
        new(
            group.Id,
            group.Name,
            group.Description,
            storage.GetUrl(group.Image),
            group.UserId,
            group.Memberships.Count);

    public Task BulkAsync(BulkRequest request, CancellationToken ct = default)
        => BulkHelper.ApplyAsync<Group>(db, request, ct);

    public async Task<IReadOnlyList<AssignmentLogResponse>> ListSubjectLogsAsync(long groupId, CancellationToken ct = default)
    {
        await EnsureGroupExistsAsync(groupId, ct);
        return await db.MainCategoryAssignmentLogs.AsNoTracking()
            .Where(x => x.GroupId == groupId)
            .OrderByDescending(x => x.Id)
            .Select(x => new AssignmentLogResponse(x.Id, x.SubjectId, x.GroupId, x.Action, x.CreatedAt))
            .ToListAsync(ct);
    }
}
