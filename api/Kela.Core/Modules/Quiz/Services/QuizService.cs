using Kela.Core.Common.Pagination;
using Kela.Core.Common.Responses;
using Kela.Core.Common.Bulk;
using Kela.Core.Common.Exceptions;
using Kela.Core.Common.Sorting;
using Kela.Core.Data;
using Kela.Core.Common.Storage;
using Kela.Core.Modules.Question.Responses;
using Kela.Core.Modules.Quiz.Requests;
using Kela.Core.Modules.Quiz.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Quiz.Services;

using Quiz = Kela.Core.Modules.Quiz.Entities.Quiz;
using QuizQuestion = Kela.Core.Modules.Quiz.Entities.QuizQuestion;
using Kela.Core.Common.Queries;
using Kela.Core.Common.Authorization;

public sealed class QuizService(AppDbContext db, IFileStorage storage) : IQuizService
{
    public async Task<PagedResponse<QuizResponse>> ListAsync(QuizListRequest request, CancellationToken ct = default)
    {

        var query = db.Quizzes.AsNoTracking();

        query = query.ApplyTrashFilter(request.IsDeleted);

        if (request.LessonId.HasValue)
        {
            query = query.Where(x => x.LessonId == request.LessonId);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == request.UserId);
        }

        query = query.SearchBy(request.Search, x => x.Title);

        return await query
            .Include(x => x.Questions).Include(x => x.Lesson)
            .ApplyListSort(request, x => x.Title, x => x.SortOrder)
        .ToPagedAsync(request, ToResponse, ct);
    }

    public async Task<QuizResponse> GetAsync(long id, long? ownerId = null, CancellationToken ct = default)
    {
        var quiz = await db.Quizzes.IgnoreQueryFilters(new[] { "SoftDelete" }).AsNoTracking()
            .Include(x => x.Questions).Include(x => x.Lesson)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Quiz not found: {id}");

        OwnershipGuard.EnsureReadable(ownerId, quiz.UserId, quiz.UserId == null, "You do not have access to this quiz");
        return ToResponse(quiz);
    }

    public async Task<QuizResponse> CreateAsync(QuizUpsertRequest request, CancellationToken ct = default)
    {
        if (request.LessonId.HasValue)
        {
            await EnsureLessonExistsAsync(request.LessonId.Value, ct);
            if (request.UserId is long ownerId) await EnsureLessonAccessibleAsync(request.LessonId.Value, ownerId, ct);
        }

        var quiz = new Quiz
        {
            LessonId = request.LessonId,
            Title = request.Title,
            DurationMinutes = request.DurationMinutes,
            Type = request.Type,
            IsActive = request.IsActive,
            IsRepeat = request.IsRepeat,
            SortOrder = request.SortOrder,
            UserId = request.UserId
        };

        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync(ct);

        return ToResponse(quiz);
    }

    public async Task<QuizResponse> UpdateAsync(long id, QuizUpsertRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        var quiz = await db.Quizzes.IgnoreQueryFilters(new[] { "SoftDelete" })
            .Include(x => x.Questions).Include(x => x.Lesson)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Quiz not found: {id}");

        OwnershipGuard.EnsureOwned(ownerId, quiz.UserId, "You can only modify your own quiz");

        if (request.LessonId.HasValue)
        {
            await EnsureLessonExistsAsync(request.LessonId.Value, ct);
            if (ownerId.HasValue) await EnsureLessonAccessibleAsync(request.LessonId.Value, ownerId.Value, ct);
        }

        quiz.LessonId = request.LessonId;
        quiz.Title = request.Title;
        quiz.DurationMinutes = request.DurationMinutes;
        quiz.Type = request.Type;
        quiz.IsActive = request.IsActive;
        quiz.IsRepeat = request.IsRepeat;
        quiz.SortOrder = request.SortOrder;
        await db.SaveChangesAsync(ct);

        return ToResponse(quiz);
    }

    public async Task DeleteAsync(long id, long? ownerId = null, CancellationToken ct = default)
    {
        var quiz = await db.Quizzes.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Quiz not found: {id}");

        OwnershipGuard.EnsureOwned(ownerId, quiz.UserId, "You can only modify your own quiz");

        quiz.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<QuestionResponse>> ListQuestionsAsync(long quizId, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureQuizAccessibleAsync(quizId, ownerId, ct);
        return await QuestionsQuery(quizId, ct);
    }

    public async Task<IReadOnlyList<QuestionResponse>> AssignQuestionsAsync(
        long quizId, AssignQuestionsRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureQuizOwnedAsync(quizId, ownerId, ct);

        foreach (var questionId in request.QuestionIds.Distinct())
        {
            var question = await db.Questions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == questionId, ct)
                ?? throw new ArgumentException($"Question not found: {questionId}");

            // Teacher yalnız öz və ya adminin sualını imtahana əlavə edə bilər.
            OwnershipGuard.EnsureReadable(ownerId, question.UserId, question.UserId == null,
                $"You do not have access to question {questionId}");

            var existing = await db.QuizQuestions.IgnoreQueryFilters(new[] { "SoftDelete" })
                .FirstOrDefaultAsync(x => x.QuizId == quizId && x.QuestionId == questionId, ct);

            if (existing is null)
            {
                db.QuizQuestions.Add(new QuizQuestion { QuizId = quizId, QuestionId = questionId });
            }
            else
            {
                existing.DeletedAt = null;
            }
        }

        await db.SaveChangesAsync(ct);
        return await ListQuestionsAsync(quizId, ownerId, ct);
    }

    public async Task RemoveQuestionAsync(long quizId, long questionId, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureQuizOwnedAsync(quizId, ownerId, ct);

        var link = await db.QuizQuestions
            .FirstOrDefaultAsync(x => x.QuizId == quizId && x.QuestionId == questionId, ct)
            ?? throw new KeyNotFoundException($"Question {questionId} is not in quiz {quizId}");

        link.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task EnsureLessonExistsAsync(long lessonId, CancellationToken ct)
    {
        if (!await db.Lessons.AnyAsync(x => x.Id == lessonId, ct))
        {
            throw new ArgumentException($"Lesson not found: {lessonId}");
        }
    }

    private async Task EnsureQuizExistsAsync(long quizId, CancellationToken ct)
    {
        if (!await db.Quizzes.AnyAsync(x => x.Id == quizId, ct))
        {
            throw new KeyNotFoundException($"Quiz not found: {quizId}");
        }
    }

    private async Task<List<QuestionResponse>> QuestionsQuery(long quizId, CancellationToken ct)
    {
        var questions = await (
            from qq in db.QuizQuestions.AsNoTracking()
            join q in db.Questions.AsNoTracking() on qq.QuestionId equals q.Id
            where qq.QuizId == quizId
            orderby qq.SortOrder, qq.Id
            select q).ToListAsync(ct);

        return questions.Select(MapQuestion).ToList();
    }

    private QuestionResponse MapQuestion(Modules.Question.Entities.Question q) =>
        new(
            q.Id, q.SubjectId,
            q.Title, q.TitleType, ResolveUrl(q.TitleType, q.Title),
            q.VariantA, ResolveUrl(q.VariantType, q.VariantA),
            q.VariantB, ResolveUrl(q.VariantType, q.VariantB),
            q.VariantC, ResolveUrl(q.VariantType, q.VariantC),
            q.VariantD, ResolveUrl(q.VariantType, q.VariantD),
            q.VariantE, ResolveUrl(q.VariantType, q.VariantE),
            q.CorrectAnswer, q.VariantType);

    private string? ResolveUrl(string type, string? value)
        => type == "image" && !string.IsNullOrWhiteSpace(value) ? storage.GetUrl(value) : null;

    private static QuizResponse ToResponse(Quiz quiz) =>
        new(quiz.Id, quiz.LessonId, quiz.Title, quiz.DurationMinutes, quiz.Type, quiz.IsActive, quiz.IsRepeat,
            quiz.SortOrder, quiz.Questions.Count, quiz.Lesson?.Title);

    public Task SetActiveAsync(long id, bool isActive, long? ownerId = null, CancellationToken ct = default)
        => db.SetActiveAsync<Quiz>(id, isActive, ownerId, x => x.UserId, "You can only modify your own quiz", ct);

    public Task BulkAsync(BulkRequest request, long? ownerId = null, CancellationToken ct = default)
        => db.BulkOwnedAsync<Quiz>(request, ownerId, x => x.UserId, "You can only modify your own quizzes", ct);

    public Task ReorderAsync(SortRequest request, long? ownerId = null, CancellationToken ct = default)
        => db.ReorderOwnedAsync<Quiz>(request, ownerId, x => x.UserId, "You can only modify your own quizzes", ct);

    private async Task EnsureLessonAccessibleAsync(long lessonId, long ownerId, CancellationToken ct)
    {
        var lesson = await db.Lessons.AsNoTracking().FirstOrDefaultAsync(x => x.Id == lessonId, ct);
        if (lesson is null || (lesson.UserId != ownerId && lesson.UserId != null))
        {
            throw new ForbiddenException("You do not have access to this lesson");
        }
    }

    private async Task EnsureQuizAccessibleAsync(long quizId, long? ownerId, CancellationToken ct)
    {
        if (!ownerId.HasValue) return;

        var owner = await db.Quizzes.AsNoTracking().Where(x => x.Id == quizId)
            .Select(x => (long?)x.UserId).FirstOrDefaultAsync(ct);

        OwnershipGuard.EnsureReadable(ownerId, owner, owner == null, "You do not have access to this quiz");
    }

    private async Task EnsureQuizOwnedAsync(long quizId, long? ownerId, CancellationToken ct)
    {
        if (!ownerId.HasValue) return;

        var owner = await db.Quizzes.AsNoTracking().Where(x => x.Id == quizId)
            .Select(x => (long?)x.UserId).FirstOrDefaultAsync(ct);

        OwnershipGuard.EnsureOwned(ownerId, owner, "You can only modify your own quiz");
    }

}
