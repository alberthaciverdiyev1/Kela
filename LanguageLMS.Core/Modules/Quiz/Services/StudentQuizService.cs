using LanguageLMS.Core.Common.Exceptions;
using LanguageLMS.Core.Data;
using LanguageLMS.Core.Modules.Quiz.Requests;
using LanguageLMS.Core.Modules.Quiz.Responses;
using LanguageLMS.Core.Modules.StudentApp.Services;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Modules.Quiz.Services;

using StudentQuiz = LanguageLMS.Core.Modules.Quiz.Entities.StudentQuiz;
using StudentQuizQuestion = LanguageLMS.Core.Modules.Quiz.Entities.StudentQuizQuestion;

public sealed class StudentQuizService(
    AppDbContext db,
    IStudentAccessService access) : IStudentQuizService
{
    public async Task<StartQuizResponse> StartAsync(long studentId, StartQuizRequest request, CancellationToken ct = default)
    {
        var quiz = await db.Quizzes.AsNoTracking().FirstOrDefaultAsync(q => q.Id == request.QuizId && q.IsActive, ct)
            ?? throw new KeyNotFoundException($"Quiz not found: {request.QuizId}");

        // Ümumi (general) imtahanların lesson_id-si olmur — bu halda fənn yoxlaması aparılmır.
        if (quiz.LessonId is not null)
        {
            var lesson = await db.Lessons.AsNoTracking().FirstOrDefaultAsync(l => l.Id == quiz.LessonId, ct)
                ?? throw new KeyNotFoundException($"Lesson not found: {quiz.LessonId}");

            if (!await access.HasSubjectAccessAsync(studentId, lesson.SubjectId, ct))
            {
                throw new ForbiddenException("You do not have access to this quiz");
            }
        }

        var completedAttempts = await db.StudentQuizzes.CountAsync(
            a => a.StudentId == studentId && a.QuizId == quiz.Id && a.IsCompleted, ct);

        if (!quiz.IsRepeat && completedAttempts > 0)
        {
            throw new ArgumentException("This quiz cannot be repeated");
        }

        var totalAttempts = await db.StudentQuizzes.CountAsync(
            a => a.StudentId == studentId && a.QuizId == quiz.Id, ct);

        var questions = await QuestionsForQuiz(quiz.Id).ToListAsync(ct);

        var attempt = new StudentQuiz
        {
            StudentId = studentId,
            QuizId = quiz.Id,
            AttemptTime = totalAttempts + 1,
            TotalQuestionCount = questions.Count,
            CompletedQuestionCount = 0,
            IsCompleted = false,
            StartDate = DateTimeOffset.UtcNow
        };

        db.StudentQuizzes.Add(attempt);
        await db.SaveChangesAsync(ct);

        return new StartQuizResponse(
            attempt.Id,
            attempt.AttemptTime,
            quiz.Id,
            quiz.Title,
            quiz.DurationMinutes,
            questions.Count,
            questions);
    }

    public async Task<CompleteQuizResponse> CompleteAsync(long studentId, CompleteQuizRequest request, CancellationToken ct = default)
    {
        var attempt = await db.StudentQuizzes
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId && a.StudentId == studentId, ct)
            ?? throw new KeyNotFoundException($"Attempt not found: {request.AttemptId}");

        if (attempt.IsCompleted)
        {
            throw new ArgumentException("This quiz has already been completed");
        }

        var quizQuestionIds = await db.QuizQuestions
            .AsNoTracking()
            .Where(qq => qq.QuizId == attempt.QuizId)
            .Select(qq => qq.QuestionId)
            .ToListAsync(ct);

        var questions = await db.Questions.AsNoTracking()
            .Where(q => quizQuestionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id, ct);

        var correct = 0;
        var answeredIds = new HashSet<long>();

        foreach (var answer in request.Answers)
        {
            if (!questions.TryGetValue(answer.QuestionId, out var question) || !answeredIds.Add(answer.QuestionId))
            {
                continue;
            }

            var isRight = string.Equals(
                answer.Answer?.Trim().ToUpperInvariant(),
                question.CorrectAnswer.Trim().ToUpperInvariant(),
                StringComparison.Ordinal);

            if (isRight) correct++;

            db.StudentQuizQuestions.Add(new StudentQuizQuestion
            {
                StudentQuizId = attempt.Id,
                QuestionId = answer.QuestionId,
                StudentAnswer = answer.Answer,
                IsRight = isRight
            });
        }

        attempt.TotalQuestionCount = quizQuestionIds.Count;
        attempt.CompletedQuestionCount = answeredIds.Count;
        attempt.IsCompleted = true;
        attempt.EndDate = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return new CompleteQuizResponse(
            attempt.Id,
            attempt.TotalQuestionCount,
            correct,
            Percent(correct, attempt.TotalQuestionCount),
            true);
    }

    public async Task<IReadOnlyList<QuizResultSummary>> ListResultsAsync(
        long studentId, DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default)
    {
        var query = db.StudentQuizzes.AsNoTracking()
            .Where(a => a.StudentId == studentId && a.IsCompleted);

        if (from is not null)
        {
            query = query.Where(a => a.StartDate >= from);
        }

        if (to is not null)
        {
            query = query.Where(a => a.StartDate < to);
        }

        var attempts = await query
            .OrderByDescending(a => a.StartDate)
            .ThenByDescending(a => a.Id)
            .ToListAsync(ct);

        var attemptIds = attempts.Select(a => a.Id).ToList();
        var quizIds = attempts.Select(a => a.QuizId).Distinct().ToList();

        var quizTitles = await db.Quizzes.AsNoTracking()
            .Where(q => quizIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id, q => q.Title, ct);

        var correctCounts = await db.StudentQuizQuestions.AsNoTracking()
            .Where(x => attemptIds.Contains(x.StudentQuizId) && x.IsRight)
            .GroupBy(x => x.StudentQuizId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

        return attempts.Select(a =>
        {
            var correct = correctCounts.GetValueOrDefault(a.Id);
            var duration = a.StartDate.HasValue && a.EndDate.HasValue
                ? (int?)(int)(a.EndDate.Value - a.StartDate.Value).TotalSeconds
                : null;

            return new QuizResultSummary(
                a.Id,
                a.AttemptTime,
                a.QuizId,
                quizTitles.GetValueOrDefault(a.QuizId, string.Empty),
                a.TotalQuestionCount,
                correct,
                Percent(correct, a.TotalQuestionCount),
                a.IsCompleted,
                a.StartDate,
                duration);
        }).ToList();
    }

    public async Task<QuizResultDetail> GetResultAsync(long studentId, long attemptId, CancellationToken ct = default)
    {
        var attempt = await db.StudentQuizzes.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.StudentId == studentId, ct)
            ?? throw new KeyNotFoundException($"Attempt not found: {attemptId}");

        var quizTitle = await db.Quizzes.AsNoTracking()
            .Where(q => q.Id == attempt.QuizId).Select(q => q.Title).FirstOrDefaultAsync(ct) ?? string.Empty;

        var answers = await db.StudentQuizQuestions.AsNoTracking()
            .Where(x => x.StudentQuizId == attemptId)
            .OrderBy(x => x.Id)
            .ToListAsync(ct);

        var questionIds = answers.Select(a => a.QuestionId).ToList();
        var questions = await db.Questions.AsNoTracking()
            .Where(q => questionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id, ct);

        var items = answers.Select(a =>
        {
            var question = questions.GetValueOrDefault(a.QuestionId);
            return new QuizResultItem(
                a.QuestionId,
                question?.Title ?? string.Empty,
                question?.VariantA,
                question?.VariantB,
                question?.VariantC,
                question?.VariantD,
                question?.VariantE,
                a.StudentAnswer,
                question?.CorrectAnswer ?? string.Empty,
                a.IsRight);
        }).ToList();

        var correct = answers.Count(a => a.IsRight);

        return new QuizResultDetail(
            attempt.Id,
            attempt.QuizId,
            quizTitle,
            attempt.TotalQuestionCount,
            correct,
            Percent(correct, attempt.TotalQuestionCount),
            attempt.IsCompleted,
            attempt.StartDate,
            attempt.EndDate,
            items);
    }

    private IQueryable<QuizQuestionForStudent> QuestionsForQuiz(long quizId) =>
        from qq in db.QuizQuestions.AsNoTracking()
        join q in db.Questions.AsNoTracking() on qq.QuestionId equals q.Id
        where qq.QuizId == quizId
        orderby qq.SortOrder, qq.Id
        select new QuizQuestionForStudent(q.Id, q.Title, q.VariantA, q.VariantB, q.VariantC, q.VariantD, q.VariantE);

    private static int Percent(int part, int total) => total > 0 ? (int)Math.Round(part * 100.0 / total) : 0;
}
