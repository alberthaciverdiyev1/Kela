using Edutor.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.Homework.Services;

public sealed record HomeworkQuizResult(long AttemptId, int CorrectCount, int TotalQuestions)
{
    public int ScorePercent => TotalQuestions == 0 ? 0 : (int)Math.Round(CorrectCount * 100.0 / TotalQuestions);
}

/// <summary>Quiz üzrə tələbənin ən son tamamlanmış cəhdini oxuyur (ayrıca cəhd yaratmır).</summary>
internal static class HomeworkQuizReader
{
    public static async Task<Dictionary<(long QuizId, long StudentId), HomeworkQuizResult>> ReadAsync(
        AppDbContext db, IReadOnlyList<long> quizIds, IReadOnlyList<long> studentIds, CancellationToken ct)
    {
        if (quizIds.Count == 0 || studentIds.Count == 0) return [];

        var attempts = await db.StudentQuizzes.AsNoTracking()
            .Where(x => quizIds.Contains(x.QuizId) && studentIds.Contains(x.StudentId) && x.IsCompleted)
            .OrderByDescending(x => x.Id)
            .Select(x => new { x.Id, x.QuizId, x.StudentId })
            .ToListAsync(ct);

        var latest = attempts
            .GroupBy(x => (x.QuizId, x.StudentId))
            .ToDictionary(g => g.Key, g => g.First().Id);

        var attemptIds = latest.Values.ToList();

        var stats = await db.StudentQuizQuestions.AsNoTracking()
            .Where(q => attemptIds.Contains(q.StudentQuizId))
            .GroupBy(q => q.StudentQuizId)
            .Select(g => new { AttemptId = g.Key, Total = g.Count(), Correct = g.Count(x => x.IsRight) })
            .ToListAsync(ct);

        var byAttempt = stats.ToDictionary(x => x.AttemptId, x => x);

        return latest.ToDictionary(
            kv => kv.Key,
            kv =>
            {
                var stat = byAttempt.GetValueOrDefault(kv.Value);
                return new HomeworkQuizResult(kv.Value, stat?.Correct ?? 0, stat?.Total ?? 0);
            });
    }
}
