using System.Text.Json;
using Kela.Core.Common.Authorization;
using Kela.Core.Common.Content;
using Kela.Core.Common.Exceptions;
using Kela.Core.Common.Localization;
using Kela.Core.Common.Pagination;
using Kela.Core.Common.Queries;
using Kela.Core.Common.Responses;
using Kela.Core.Common.Storage;
using Kela.Core.Data;
using Kela.Core.Modules.Homework.Abstractions;
using Kela.Core.Modules.Homework.Requests;
using Kela.Core.Modules.Homework.Responses;
using Kela.Core.Modules.Notification.Abstractions;
using Kela.Core.Modules.Notification.Requests;
using Kela.Core.Modules.Subscription.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Kela.Core.Modules.Homework.Services;

using Homework = Kela.Core.Modules.Homework.Entities.Homework;
using HomeworkAssignment = Kela.Core.Modules.Homework.Entities.HomeworkAssignment;
using HomeworkGroup = Kela.Core.Modules.Homework.Entities.HomeworkGroup;
using HomeworkQuiz = Kela.Core.Modules.Homework.Entities.HomeworkQuiz;
using HomeworkStatus = Kela.Core.Modules.Homework.Entities.HomeworkStatus;

public sealed class HomeworkService(
    AppDbContext db,
    IFileStorage storage,
    ISubscriptionService subscription,
    INotificationService notifications,
    IStringLocalizer<SharedResource> L) : IHomeworkService
{
    public async Task<PagedResponse<HomeworkResponse>> ListAsync(
        HomeworkListRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        var query = db.Homeworks.AsNoTracking().ApplyTrashFilter(request.IsDeleted);

        if (ownerId is long teacherId) query = query.Where(x => x.UserId == teacherId);
        if (request.GroupId is long groupId) query = query.Where(x => x.Groups.Any(g => g.GroupId == groupId));
        if (request.IsPublished is bool published) query = query.Where(x => x.IsPublished == published);

        query = query.SearchBy(request.Search, x => x.Title);

        var (page, perPage) = Paging.Normalize(request, defaultPerPage: 50);
        var total = await query.CountAsync(ct);

        var items = await query
            .Include(x => x.Groups).ThenInclude(g => g.Group)
            .Include(x => x.Quizzes).ThenInclude(q => q.Quiz)
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * perPage).Take(perPage)
            .ToListAsync(ct);

        var counts = await CountsAsync(items.Select(x => x.Id).ToList(), ct);
        var quizzes = await QuizInfoAsync(items.SelectMany(x => x.Quizzes).Select(x => x.QuizId).Distinct().ToList(), ct);

        var data = items.Select(x => ToResponse(x, counts.GetValueOrDefault(x.Id), quizzes)).ToList();
        return new PagedResponse<HomeworkResponse>(data, page, Paging.LastPage(total, perPage), perPage, total);
    }

    public async Task<HomeworkResponse> GetAsync(long id, long? ownerId = null, CancellationToken ct = default)
    {
        var homework = await LoadAsync(id, ownerId, tracked: false, ct);

        var counts = await CountsAsync([homework.Id], ct);
        var quizzes = await QuizInfoAsync(homework.Quizzes.Select(x => x.QuizId).ToList(), ct);

        return ToResponse(homework, counts.GetValueOrDefault(homework.Id), quizzes);
    }

    public async Task<HomeworkResponse> CreateAsync(
        HomeworkUpsertRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureWritableAsync(ct);

        var groups = await ValidateGroupsAsync(request.GroupIds, ownerId, ct);
        var quizzes = await ValidateQuizzesAsync(request.QuizIds, ownerId, ct);
        var settings = ReadGroupSettings(request);

        var homework = new Homework
        {
            UserId = ownerId ?? request.UserId,
            Title = request.Title.Trim(),
            Description = SanitizeDescription(request.Description)
        };

        foreach (var (groupId, dueDate, maxScore) in settings)
        {
            homework.Groups.Add(new HomeworkGroup { GroupId = groupId, DueDate = dueDate, MaxScore = maxScore });
        }
        for (var i = 0; i < quizzes.Count; i++) homework.Quizzes.Add(new HomeworkQuiz { QuizId = quizzes[i], SortOrder = i });

        db.Homeworks.Add(homework);
        await db.SaveChangesAsync(ct);

        return await GetAsync(homework.Id, ownerId, ct);
    }

    public async Task<HomeworkResponse> UpdateAsync(
        long id, HomeworkUpsertRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        var homework = await LoadAsync(id, ownerId, tracked: true, ct);
        await EnsureWritableAsync(ct);

        var settings = ReadGroupSettings(request);

        if (homework.IsPublished)
        {
            // Yayımdan sonra hədəflər və qrup şərtləri dəyişmir — tələbələr snapshot alıb.
            var current = homework.Groups
                .OrderBy(x => x.GroupId)
                .Select(x => (x.GroupId, x.DueDate, x.MaxScore));
            var groupsChanged = !current.SequenceEqual(settings.OrderBy(x => x.GroupId));
            var quizzesChanged = !homework.Quizzes.Select(x => x.QuizId).Order().SequenceEqual(request.QuizIds.Distinct().Order());

            if (groupsChanged || quizzesChanged)
            {
                throw new ArgumentException(L["Homework_PublishedTargetsLocked"]);
            }
        }
        else
        {
            await ValidateGroupsAsync(request.GroupIds, ownerId, ct);
            var quizzes = await ValidateQuizzesAsync(request.QuizIds, ownerId, ct);

            db.HomeworkGroups.RemoveRange(homework.Groups);
            db.HomeworkQuizzes.RemoveRange(homework.Quizzes);
            homework.Groups.Clear();
            homework.Quizzes.Clear();

            foreach (var (groupId, dueDate, maxScore) in settings)
            {
                homework.Groups.Add(new HomeworkGroup { GroupId = groupId, DueDate = dueDate, MaxScore = maxScore });
            }

            for (var i = 0; i < quizzes.Count; i++) homework.Quizzes.Add(new HomeworkQuiz { QuizId = quizzes[i], SortOrder = i });
        }

        homework.Title = request.Title.Trim();
        homework.Description = SanitizeDescription(request.Description);

        await db.SaveChangesAsync(ct);
        return await GetAsync(homework.Id, ownerId, ct);
    }

    public async Task DeleteAsync(long id, long? ownerId = null, CancellationToken ct = default)
    {
        var homework = await LoadAsync(id, ownerId, tracked: true, ct);
        homework.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<HomeworkResponse> PublishAsync(long id, long? ownerId = null, CancellationToken ct = default)
    {
        var homework = await LoadAsync(id, ownerId, tracked: true, ct);

        if (homework.IsPublished) return await GetAsync(homework.Id, ownerId, ct);
        if (homework.Groups.Count == 0) throw new ArgumentException(L["Homework_GroupRequired"]);

        await EnsureWritableAsync(ct);

        var groupIds = homework.Groups.Select(x => x.GroupId).ToList();

        var members = await db.StudentGroups.AsNoTracking()
            .Where(x => groupIds.Contains(x.GroupId))
            .Select(x => new { x.StudentId, x.GroupId })
            .ToListAsync(ct);

        var targetGroups = homework.Groups.ToDictionary(x => x.GroupId, x => (x.DueDate, x.MaxScore));

        // Bir tələbə bir neçə qrupdadırsa, ən erkən son tarixli qrupun şərtləri götürülür.
        var students = members
            .GroupBy(x => x.StudentId)
            .Select(g =>
            {
                var picked = g.Select(x => x.GroupId).OrderBy(id => targetGroups[id].DueDate).First();
                return new
                {
                    StudentId = g.Key,
                    GroupId = picked,
                    targetGroups[picked].DueDate,
                    targetGroups[picked].MaxScore
                };
            })
            .ToList();

        foreach (var student in students)
        {
            db.HomeworkAssignments.Add(new HomeworkAssignment
            {
                HomeworkId = homework.Id,
                StudentId = student.StudentId,
                GroupId = student.GroupId,
                DueDate = student.DueDate,
                MaxScore = student.MaxScore,
                Status = HomeworkStatus.Assigned
            });
        }

        homework.IsPublished = true;
        homework.PublishedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        if (students.Count > 0)
        {
            await NotifyAsync(homework, students.Select(x => x.StudentId).ToList(), "published", ct);
        }

        return await GetAsync(homework.Id, ownerId, ct);
    }

    public async Task<PagedResponse<HomeworkAssignmentResponse>> ListAssignmentsAsync(
        long homeworkId, HomeworkAssignmentListRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        var homework = await LoadAsync(homeworkId, ownerId, tracked: false, ct);

        var query = db.HomeworkAssignments.AsNoTracking()
            .Where(x => x.HomeworkId == homeworkId)
            .Include(x => x.Student)
            .Include(x => x.Files)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
        query = query.SearchBy(request.Search, x => x.Student!.FullName);

        var (page, perPage) = Paging.Normalize(request, defaultPerPage: 100);
        var total = await query.CountAsync(ct);

        var items = await query.OrderBy(x => x.Student!.FullName)
            .Skip((page - 1) * perPage).Take(perPage).ToListAsync(ct);

        var quizIds = homework.Quizzes.Select(x => x.QuizId).ToList();
        var quizInfo = await QuizInfoAsync(quizIds, ct);
        var results = await HomeworkQuizReader.ReadAsync(db, quizIds, items.Select(x => x.StudentId).ToList(), ct);

        var data = items.Select(a => new HomeworkAssignmentResponse(
            a.StudentId,
            a.Student?.FullName ?? string.Empty,
            a.Student?.Email ?? string.Empty,
            a.Status,
            a.Text,
            a.Link,
            a.SubmittedAt,
            a.IsLate,
            a.AttemptCount,
            a.Score,
            a.Feedback,
            a.GradedAt,
            a.ReturnedAt,
            a.Files.OrderBy(f => f.Id).Select(ToFileResponse).ToList(),
            BuildQuizResults(homework.Quizzes.OrderBy(q => q.SortOrder).ToList(), quizInfo, results, a.StudentId))).ToList();

        return new PagedResponse<HomeworkAssignmentResponse>(data, page, Paging.LastPage(total, perPage), perPage, total);
    }

    public async Task<HomeworkAssignmentResponse> GradeAsync(
        long homeworkId, long studentId, GradeHomeworkRequest request, long? ownerId = null,
        long? graderId = null, CancellationToken ct = default)
    {
        var homework = await LoadAsync(homeworkId, ownerId, tracked: false, ct);
        await EnsureWritableAsync(ct);

        if (!homework.IsPublished) throw new ArgumentException(L["Homework_NotPublished"]);

        var assignment = await db.HomeworkAssignments
            .Include(x => x.Student).Include(x => x.Files)
            .FirstOrDefaultAsync(x => x.HomeworkId == homeworkId && x.StudentId == studentId, ct)
            ?? throw new KeyNotFoundException($"Assignment not found: {studentId}");

        if (assignment.Status == HomeworkStatus.Assigned)
        {
            throw new ArgumentException(L["Homework_NotSubmitted"]);
        }

        if (request.Score < 0 || request.Score > assignment.MaxScore)
        {
            throw new ArgumentException(string.Format(L["Homework_ScoreOutOfRange"], assignment.MaxScore));
        }

        var now = DateTimeOffset.UtcNow;
        assignment.Score = request.Score;
        assignment.Feedback = string.IsNullOrWhiteSpace(request.Feedback) ? null : request.Feedback.Trim();
        assignment.GradedAt = now;
        assignment.GradedByUserId = graderId;

        if (request.ReturnToStudent)
        {
            assignment.Status = HomeworkStatus.Returned;
            assignment.ReturnedAt = now;
        }
        else
        {
            assignment.Status = HomeworkStatus.Graded;
            assignment.ReturnedAt = null;
        }

        await db.SaveChangesAsync(ct);
        await NotifyAsync(homework, [studentId], request.ReturnToStudent ? "returned" : "graded", ct);

        var quizIds = homework.Quizzes.Select(x => x.QuizId).ToList();
        var quizInfo = await QuizInfoAsync(quizIds, ct);
        var results = await HomeworkQuizReader.ReadAsync(db, quizIds, [studentId], ct);

        return new HomeworkAssignmentResponse(
            assignment.StudentId,
            assignment.Student?.FullName ?? string.Empty,
            assignment.Student?.Email ?? string.Empty,
            assignment.Status,
            assignment.Text,
            assignment.Link,
            assignment.SubmittedAt,
            assignment.IsLate,
            assignment.AttemptCount,
            assignment.Score,
            assignment.Feedback,
            assignment.GradedAt,
            assignment.ReturnedAt,
            assignment.Files.OrderBy(f => f.Id).Select(ToFileResponse).ToList(),
            BuildQuizResults(homework.Quizzes.OrderBy(q => q.SortOrder).ToList(), quizInfo, results, studentId));
    }

    // ---- Daxili ----

    private async Task<Homework> LoadAsync(long id, long? ownerId, bool tracked, CancellationToken ct)
    {
        var query = tracked ? db.Homeworks : db.Homeworks.AsNoTracking();

        var homework = await query
            .Include(x => x.Groups).ThenInclude(g => g.Group)
            .Include(x => x.Quizzes).ThenInclude(q => q.Quiz)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Homework not found: {id}");

        OwnershipGuard.EnsureOwned(ownerId, homework.UserId, "You can only manage your own homework");
        return homework;
    }

    private async Task EnsureWritableAsync(CancellationToken ct)
    {
        if (db.CurrentTenantId is long tenantId) await subscription.EnsureWritableAsync(tenantId, ct);
    }

    private List<(long GroupId, DateTimeOffset DueDate, decimal MaxScore)> ReadGroupSettings(HomeworkUpsertRequest request)
    {
        if (request.GroupIds.Count != request.GroupDueDates.Count
            || request.GroupIds.Count != request.GroupScores.Count)
        {
            throw new ArgumentException(L["Homework_GroupDeadlineRequired"]);
        }

        var settings = new List<(long, DateTimeOffset, decimal)>();
        var seen = new HashSet<long>();

        for (var i = 0; i < request.GroupIds.Count; i++)
        {
            if (!seen.Add(request.GroupIds[i])) continue;

            var dueDate = request.GroupDueDates[i];
            var maxScore = request.GroupScores[i];

            if (dueDate == default) throw new ArgumentException(L["Homework_GroupDeadlineRequired"]);
            if (maxScore <= 0 || maxScore > 1000) throw new ArgumentException(L["Homework_MaxScoreInvalid"]);

            settings.Add((request.GroupIds[i], dueDate, maxScore));
        }

        return settings;
    }

    private async Task<List<long>> ValidateGroupsAsync(IEnumerable<long> groupIds, long? ownerId, CancellationToken ct)
    {
        var ids = groupIds.Distinct().ToList();
        if (ids.Count == 0) return ids;

        var groups = await db.Groups.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new { x.Id, x.UserId })
            .ToListAsync(ct);

        if (groups.Count != ids.Count) throw new ArgumentException(L["Homework_GroupNotFound"]);

        if (ownerId is long teacherId && groups.Any(x => x.UserId != teacherId))
        {
            throw new ForbiddenException("You can only assign homework to your own groups");
        }

        return ids;
    }

    private async Task<List<long>> ValidateQuizzesAsync(IEnumerable<long> quizIds, long? ownerId, CancellationToken ct)
    {
        var ids = quizIds.Distinct().ToList();
        if (ids.Count == 0) return ids;

        var quizzes = await db.Quizzes.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new { x.Id, x.UserId })
            .ToListAsync(ct);

        if (quizzes.Count != ids.Count) throw new ArgumentException(L["Homework_QuizNotFound"]);

        // Müəllim yalnız öz və ya admin yaratdığı quiz-i əlavə edə bilər.
        if (ownerId is long teacherId && quizzes.Any(x => x.UserId is long owner && owner != teacherId))
        {
            throw new ForbiddenException("You can only attach your own quizzes");
        }

        return ids;
    }

    private async Task<Dictionary<long, HomeworkCounts>> CountsAsync(IReadOnlyList<long> homeworkIds, CancellationToken ct)
    {
        if (homeworkIds.Count == 0) return [];

        var rows = await db.HomeworkAssignments.AsNoTracking()
            .Where(x => homeworkIds.Contains(x.HomeworkId))
            .GroupBy(x => new { x.HomeworkId, x.Status })
            .Select(g => new { g.Key.HomeworkId, g.Key.Status, Count = g.Count() })
            .ToListAsync(ct);

        return rows
            .GroupBy(x => x.HomeworkId)
            .ToDictionary(g => g.Key, g => new HomeworkCounts(
                g.Sum(x => x.Count),
                g.Where(x => x.Status == HomeworkStatus.Submitted).Sum(x => x.Count),
                g.Where(x => x.Status == HomeworkStatus.Graded).Sum(x => x.Count),
                g.Where(x => x.Status == HomeworkStatus.Returned).Sum(x => x.Count)));
    }

    private async Task<Dictionary<long, HomeworkQuizInfoResponse>> QuizInfoAsync(IReadOnlyList<long> quizIds, CancellationToken ct)
    {
        if (quizIds.Count == 0) return [];

        var quizzes = await db.Quizzes.AsNoTracking()
            .Where(x => quizIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Title, x.DurationMinutes })
            .ToListAsync(ct);

        var counts = await db.QuizQuestions.AsNoTracking()
            .Where(x => quizIds.Contains(x.QuizId))
            .GroupBy(x => x.QuizId)
            .Select(g => new { QuizId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return quizzes.ToDictionary(
            x => x.Id,
            x => new HomeworkQuizInfoResponse(x.Id, x.Title, counts.FirstOrDefault(c => c.QuizId == x.Id)?.Count ?? 0, x.DurationMinutes));
    }

    private IReadOnlyList<HomeworkQuizResultResponse> BuildQuizResults(
        IReadOnlyList<HomeworkQuiz> quizzes,
        IReadOnlyDictionary<long, HomeworkQuizInfoResponse> info,
        IReadOnlyDictionary<(long QuizId, long StudentId), HomeworkQuizResult> results,
        long studentId)
        => quizzes.Select(q =>
        {
            var meta = info.GetValueOrDefault(q.QuizId);
            var result = results.GetValueOrDefault((q.QuizId, studentId));
            return new HomeworkQuizResultResponse(
                q.QuizId,
                meta?.Title ?? string.Empty,
                meta?.QuestionCount ?? 0,
                meta?.DurationMinutes ?? 0,
                result?.AttemptId,
                result is not null,
                result?.CorrectCount ?? 0,
                result?.ScorePercent ?? 0);
        }).ToList();

    private HomeworkResponse ToResponse(
        Homework homework,
        HomeworkCounts? counts,
        IReadOnlyDictionary<long, HomeworkQuizInfoResponse> quizInfo)
        => new(
            homework.Id,
            homework.Title,
            SafeHtml.Sanitize(homework.Description),
            homework.IsPublished,
            homework.PublishedAt,
            homework.IsActive,
            homework.UserId,
            homework.Groups.OrderBy(x => x.Id)
                .Select(x => new HomeworkGroupInfoResponse(
                    x.GroupId, x.Group?.Name ?? string.Empty, x.DueDate, x.MaxScore)).ToList(),
            homework.Quizzes.OrderBy(x => x.SortOrder).Select(x =>
            {
                var info = quizInfo.GetValueOrDefault(x.QuizId);
                return new HomeworkQuizInfoResponse(x.QuizId, info?.Title ?? string.Empty, info?.QuestionCount ?? 0, info?.DurationMinutes ?? 0);
            }).ToList(),
            counts?.Assigned ?? 0,
            counts?.Submitted ?? 0,
            counts?.Graded ?? 0,
            counts?.Returned ?? 0);

    private static string? SanitizeDescription(string? html)
        => string.IsNullOrWhiteSpace(html) ? null : SafeHtml.Sanitize(html)?.Trim();

    private HomeworkFileResponse ToFileResponse(Entities.HomeworkAssignmentFile file)
        => new(file.Id, file.FileName, storage.GetUrl(file.Path), file.ContentType, file.SizeBytes);

    private async Task NotifyAsync(Homework homework, IReadOnlyList<long> studentIds, string @event, CancellationToken ct)
    {
        var (title, body) = @event switch
        {
            "published" => (L["Homework_NotifyPublishedTitle"], string.Format(L["Homework_NotifyPublishedBody"], homework.Title)),
            "returned" => (L["Homework_NotifyReturnedTitle"], string.Format(L["Homework_NotifyReturnedBody"], homework.Title)),
            _ => (L["Homework_NotifyGradedTitle"], string.Format(L["Homework_NotifyGradedBody"], homework.Title))
        };

        await notifications.CreateAsync(new CreateNotificationRequest
        {
            Title = title,
            Body = body,
            All = false,
            StudentIds = studentIds.Distinct().ToList(),
            Data = JsonSerializer.Serialize(new
            {
                type = "homework",
                id = homework.Id,
                route = $"/homeworks/{homework.Id}",
                @event
            })
        }, homework.UserId, ct);
    }

    private sealed record HomeworkCounts(int Assigned, int Submitted, int Graded, int Returned);
}
