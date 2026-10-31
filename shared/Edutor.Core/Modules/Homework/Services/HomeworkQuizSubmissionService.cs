using Edutor.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.Homework.Services;

using HomeworkStatus = Edutor.Core.Modules.Homework.Entities.HomeworkStatus;

public sealed class HomeworkQuizSubmissionService(AppDbContext db) : IHomeworkQuizSubmissionService
{
    public async Task SyncAfterQuizCompletionAsync(long studentId, long quizId, CancellationToken ct = default)
    {
        var homeworkIds = await db.HomeworkQuizzes.AsNoTracking()
            .Where(x => x.QuizId == quizId)
            .Select(x => x.HomeworkId)
            .Distinct()
            .ToListAsync(ct);

        if (homeworkIds.Count > 0) await SyncAsync(studentId, homeworkIds, ct);
    }

    public async Task SyncHomeworkAsync(long studentId, long homeworkId, CancellationToken ct = default)
        => await SyncAsync(studentId, [homeworkId], ct);

    private async Task SyncAsync(long studentId, IReadOnlyList<long> homeworkIds, CancellationToken ct)
    {
        var assignments = await db.HomeworkAssignments
            .Where(x => x.StudentId == studentId && homeworkIds.Contains(x.HomeworkId)
                        && (x.Status == HomeworkStatus.Assigned || x.Status == HomeworkStatus.Returned))
            .ToListAsync(ct);

        if (assignments.Count == 0) return;

        var quizLinks = await db.HomeworkQuizzes.AsNoTracking()
            .Where(x => homeworkIds.Contains(x.HomeworkId))
            .Select(x => new { x.HomeworkId, x.QuizId })
            .ToListAsync(ct);

        if (quizLinks.Count == 0) return;

        var completed = (await db.StudentQuizzes.AsNoTracking()
            .Where(x => x.StudentId == studentId && x.IsCompleted)
            .Select(x => x.QuizId)
            .Distinct()
            .ToListAsync(ct)).ToHashSet();

        var now = DateTimeOffset.UtcNow;
        var changed = false;

        foreach (var assignment in assignments)
        {
            var quizIds = quizLinks.Where(x => x.HomeworkId == assignment.HomeworkId).Select(x => x.QuizId).ToList();
            if (quizIds.Count == 0 || !quizIds.All(completed.Contains)) continue;

            assignment.Status = HomeworkStatus.Submitted;
            assignment.SubmittedAt = now;
            assignment.IsLate = now > assignment.DueDate;
            assignment.AttemptCount += 1;
            changed = true;
        }

        if (changed) await db.SaveChangesAsync(ct);
    }
}
