using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Common.Storage;
using LanguageLMS.Core.Data;
using LanguageLMS.Core.Modules.Group.Requests;
using LanguageLMS.Core.Modules.Group.Responses;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Modules.Group.Services;

using Group = LanguageLMS.Core.Modules.Group.Entities.Group;
using StudentGroup = LanguageLMS.Core.Modules.Group.Entities.StudentGroup;

public sealed class GroupService(AppDbContext db, IFileStorage storage) : IGroupService
{
    private const string ImageFolder = "group";

    public async Task<PagedResponse<GroupResponse>> ListAsync(GroupListRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var perPage = request.PerPage is < 1 or > 200 ? 20 : request.PerPage;

        var query = db.Groups.AsNoTracking();

        if (request.IsDeleted == 1)
        {
            query = query.IgnoreQueryFilters().Where(x => x.DeletedAt != null);
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
            .OrderByDescending(x => x.Id)
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
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(x => x.Memberships)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Group not found: {id}");

        return ToResponse(group);
    }

    public async Task<GroupResponse> CreateAsync(GroupUpsertRequest request, CancellationToken ct = default)
    {
        var image = request.Image is not null
            ? await storage.UploadAsync(request.Image, ImageFolder, ct)
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

        return ToResponse(group);
    }

    public async Task<GroupResponse> UpdateAsync(long id, GroupUpsertRequest request, CancellationToken ct = default)
    {
        var group = await db.Groups
            .IgnoreQueryFilters()
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
            group.Image = await storage.UploadAsync(request.Image, ImageFolder, ct);
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
                .IgnoreQueryFilters()
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
}
