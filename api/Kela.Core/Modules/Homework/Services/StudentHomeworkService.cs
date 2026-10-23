using Kela.Core.Common.Localization;
using Kela.Core.Common.Storage;
using Kela.Core.Common.Exceptions;
using Kela.Core.Data;
using Kela.Core.Modules.Homework.Abstractions;
using Kela.Core.Modules.Homework.Requests;
using Kela.Core.Modules.Homework.Responses;
using Kela.Core.Modules.Subscription.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Kela.Core.Modules.Homework.Services;

using HomeworkAssignment = Kela.Core.Modules.Homework.Entities.HomeworkAssignment;
using HomeworkAssignmentFile = Kela.Core.Modules.Homework.Entities.HomeworkAssignmentFile;
using HomeworkStatus = Kela.Core.Modules.Homework.Entities.HomeworkStatus;

public sealed class StudentHomeworkService(
    AppDbContext db,
    IFileStorage storage,
    ITenantFolder folders,
    ISubscriptionService subscription,
    IStringLocalizer<SharedResource> L) : IStudentHomeworkService
{
    public async Task<IReadOnlyList<StudentHomeworkSummaryResponse>> ListAsync(long studentId, CancellationToken ct = default)
    {
        var rows = await db.HomeworkAssignments.AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .Join(db.Homeworks.Where(h => h.IsPublished && h.IsActive),
                a => a.HomeworkId, h => h.Id, (a, h) => new { Assignment = a, Homework = h })
            .OrderBy(x => x.Homework.DueDate)
            .ToListAsync(ct);

        var homeworkIds = rows.Select(x => x.Homework.Id).ToList();
        var quizLinks = await db.HomeworkQuizzes.AsNoTracking()
            .Where(x => homeworkIds.Contains(x.HomeworkId))
            .Select(x => new { x.HomeworkId, x.QuizId })
            .ToListAsync(ct);

        var quizIds = quizLinks.Select(x => x.QuizId).Distinct().ToList();

        var completed = quizIds.Count == 0
            ? []
            : await db.StudentQuizzes.AsNoTracking()
                .Where(x => x.StudentId == studentId && quizIds.Contains(x.QuizId) && x.IsCompleted)
                .Select(x => x.QuizId)
                .Distinct()
                .ToListAsync(ct);

        return rows.Select(x =>
        {
            var quizzes = quizLinks.Where(q => q.HomeworkId == x.Homework.Id).Select(q => q.QuizId).ToList();
            return new StudentHomeworkSummaryResponse(
                x.Homework.Id,
                x.Homework.Title,
                x.Homework.DueDate,
                x.Homework.MaxScore,
                x.Assignment.Status,
                x.Assignment.IsLate,
                x.Assignment.Score,
                x.Assignment.SubmittedAt,
                quizzes.Count,
                quizzes.Count(q => completed.Contains(q)));
        }).ToList();
    }

    public async Task<StudentHomeworkDetailResponse> GetAsync(long studentId, long homeworkId, CancellationToken ct = default)
    {
        var assignment = await LoadAssignmentAsync(studentId, homeworkId, tracked: false, ct);
        return await BuildDetailAsync(assignment, ct);
    }

    public async Task<StudentHomeworkDetailResponse> SubmitAsync(
        long studentId, long homeworkId, SubmitHomeworkRequest request, CancellationToken ct = default)
    {
        var assignment = await LoadAssignmentAsync(studentId, homeworkId, tracked: true, ct);
        var homework = assignment.Homework!;

        if (assignment.Status == HomeworkStatus.Graded)
        {
            throw new ArgumentException(L["Homework_AlreadyGraded"]);
        }

        if (db.CurrentTenantId is long tenantId) await subscription.EnsureWritableAsync(tenantId, ct);

        var text = string.IsNullOrWhiteSpace(request.Text) ? null : request.Text.Trim();
        var link = string.IsNullOrWhiteSpace(request.Link) ? null : request.Link.Trim();
        var files = request.Files.Where(x => x.Length > 0).ToList();

        if (text is { Length: > 10000 }) throw new ArgumentException(L["Homework_TextTooLong"]);
        if (link is not null && !HomeworkFileRules.IsHttpUrl(link)) throw new ArgumentException(L["Homework_LinkInvalid"]);
        if (files.Count > 0) HomeworkFileRules.Validate(files);

        var hasQuizzes = await db.HomeworkQuizzes.AnyAsync(x => x.HomeworkId == homeworkId, ct);
        if (!hasQuizzes && text is null && link is null && files.Count == 0)
        {
            throw new ArgumentException(L["Homework_EmptySubmission"]);
        }

        var folder = $"{await folders.ResolveAsync(homework.TenantId, ct)}/homework/{homeworkId}/{studentId}";

        var uploaded = new List<HomeworkAssignmentFile>();
        long addedBytes = 0;

        try
        {
            foreach (var file in files)
            {
                var path = await storage.UploadAsync(file, folder, ct);
                var size = await storage.GetSizeAsync(path, ct) ?? file.Length;

                uploaded.Add(new HomeworkAssignmentFile
                {
                    AssignmentId = assignment.Id,
                    FileName = Path.GetFileName(file.FileName),
                    Path = path,
                    ContentType = file.ContentType,
                    SizeBytes = size
                });

                addedBytes += size;
            }
        }
        catch
        {
            foreach (var file in uploaded) await storage.DeleteAsync(file.Path, ct);
            throw;
        }

        var oldFiles = assignment.Files.ToList();
        var freedBytes = oldFiles.Sum(x => x.SizeBytes);

        db.HomeworkAssignmentFiles.RemoveRange(oldFiles);
        assignment.Files.Clear();

        foreach (var file in uploaded) assignment.Files.Add(file);

        assignment.Text = text;
        assignment.Link = link;
        assignment.SubmittedAt = DateTimeOffset.UtcNow;
        assignment.IsLate = assignment.SubmittedAt > homework.DueDate;
        assignment.AttemptCount += 1;
        assignment.Status = HomeworkStatus.Submitted;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            foreach (var file in uploaded) await storage.DeleteAsync(file.Path, ct);
            throw;
        }

        // Köhnə fayllar yalnız DB uğurla yeniləndikdən sonra fiziki silinir.
        foreach (var file in oldFiles)
        {
            try { await storage.DeleteAsync(file.Path, ct); }
            catch (Exception) { /* növbəti gecə ölçməsi düzəldəcək */ }
        }

        if (freedBytes > 0) await subscription.SubtractStorageAsync(homework.TenantId, freedBytes, ct);
        if (addedBytes > 0) await subscription.AddStorageAsync(homework.TenantId, addedBytes, ct);

        return await BuildDetailAsync(assignment, ct);
    }

    // ---- Daxili ----

    private async Task<HomeworkAssignment> LoadAssignmentAsync(long studentId, long homeworkId, bool tracked, CancellationToken ct)
    {
        var query = tracked ? db.HomeworkAssignments : db.HomeworkAssignments.AsNoTracking();

        var assignment = await query
            .Include(x => x.Files)
            .Include(x => x.Homework).ThenInclude(h => h!.Quizzes).ThenInclude(q => q.Quiz)
            .FirstOrDefaultAsync(x => x.StudentId == studentId && x.HomeworkId == homeworkId, ct)
            ?? throw new KeyNotFoundException($"Homework not found: {homeworkId}");

        if (assignment.Homework is null || !assignment.Homework.IsPublished || !assignment.Homework.IsActive)
        {
            throw new KeyNotFoundException($"Homework not found: {homeworkId}");
        }

        return assignment;
    }

    private async Task<StudentHomeworkDetailResponse> BuildDetailAsync(HomeworkAssignment assignment, CancellationToken ct)
    {
        var homework = assignment.Homework!;
        var quizzes = homework.Quizzes
            .Where(x => x.Quiz is not null)
            .OrderBy(x => x.SortOrder)
            .ToList();

        var quizIds = quizzes.Select(x => x.QuizId).ToList();

        var questionCounts = quizIds.Count == 0
            ? []
            : await db.QuizQuestions.AsNoTracking()
                .Where(x => quizIds.Contains(x.QuizId))
                .GroupBy(x => x.QuizId)
                .Select(g => new { QuizId = g.Key, Count = g.Count() })
                .ToListAsync(ct);

        var results = await HomeworkQuizReader.ReadAsync(db, quizIds, [assignment.StudentId], ct);

        var quizResults = quizzes.Select(q =>
        {
            var result = results.GetValueOrDefault((q.QuizId, assignment.StudentId));
            var total = questionCounts.FirstOrDefault(x => x.QuizId == q.QuizId)?.Count ?? 0;
            return new HomeworkQuizResultResponse(
                q.QuizId,
                q.Quiz!.Title,
                total,
                q.Quiz.DurationMinutes,
                result?.AttemptId,
                result is not null,
                result?.CorrectCount ?? 0,
                result?.ScorePercent ?? 0);
        }).ToList();

        var files = assignment.Files.OrderBy(x => x.Id).Select(x => new HomeworkFileResponse(
            x.Id, x.FileName, storage.GetUrl(x.Path), x.ContentType, x.SizeBytes)).ToList();

        return new StudentHomeworkDetailResponse(
            homework.Id,
            homework.Title,
            homework.Description,
            homework.DueDate,
            homework.MaxScore,
            assignment.Status,
            assignment.IsLate,
            assignment.AttemptCount,
            assignment.Text,
            assignment.Link,
            assignment.SubmittedAt,
            assignment.Score,
            assignment.Feedback,
            assignment.ReturnedAt,
            files,
            quizResults,
            quizResults.Count > 0,
            quizResults.All(x => x.Completed),
            assignment.Status != HomeworkStatus.Graded);
    }
}
