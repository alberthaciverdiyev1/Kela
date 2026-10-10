using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Data;
using LanguageLMS.Core.Common.Storage;
using LanguageLMS.Core.Modules.Question.Responses;
using LanguageLMS.Core.Modules.Quiz.Requests;
using LanguageLMS.Core.Modules.Quiz.Responses;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Modules.Quiz.Services;

using Quiz = LanguageLMS.Core.Modules.Quiz.Entities.Quiz;
using QuizQuestion = LanguageLMS.Core.Modules.Quiz.Entities.QuizQuestion;

public sealed class QuizService(AppDbContext db, IFileStorage storage) : IQuizService
{
    public async Task<PagedResponse<QuizResponse>> ListAsync(QuizListRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var perPage = request.PerPage is < 1 or > 200 ? 20 : request.PerPage;

        var query = db.Quizzes.AsNoTracking();

        if (request.IsDeleted == 1)
        {
            query = query.IgnoreQueryFilters().Where(x => x.DeletedAt != null);
        }

        if (request.LessonId.HasValue)
        {
            query = query.Where(x => x.LessonId == request.LessonId);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .Include(x => x.Questions)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(ct);

        var data = items.Select(ToResponse).ToList();
        var lastPage = (int)Math.Ceiling(total / (double)perPage);

        return new PagedResponse<QuizResponse>(data, page, lastPage, perPage, total);
    }

    public async Task<QuizResponse> GetAsync(long id, CancellationToken ct = default)
    {
        var quiz = await db.Quizzes.IgnoreQueryFilters().AsNoTracking()
            .Include(x => x.Questions)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Quiz not found: {id}");

        return ToResponse(quiz);
    }

    public async Task<QuizResponse> CreateAsync(QuizUpsertRequest request, CancellationToken ct = default)
    {
        if (request.LessonId.HasValue) await EnsureLessonExistsAsync(request.LessonId.Value, ct);

        var quiz = new Quiz
        {
            LessonId = request.LessonId,
            Title = request.Title,
            DurationMinutes = request.DurationMinutes,
            Type = request.Type,
            IsActive = request.IsActive,
            IsRepeat = request.IsRepeat,
            SortOrder = request.SortOrder
        };

        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync(ct);

        return ToResponse(quiz);
    }

    public async Task<QuizResponse> UpdateAsync(long id, QuizUpsertRequest request, CancellationToken ct = default)
    {
        var quiz = await db.Quizzes.IgnoreQueryFilters()
            .Include(x => x.Questions)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Quiz not found: {id}");

        if (request.LessonId.HasValue) await EnsureLessonExistsAsync(request.LessonId.Value, ct);

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

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var quiz = await db.Quizzes.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Quiz not found: {id}");

        quiz.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<QuestionResponse>> ListQuestionsAsync(long quizId, CancellationToken ct = default)
    {
        await EnsureQuizExistsAsync(quizId, ct);
        return await QuestionsQuery(quizId, ct);
    }

    public async Task<IReadOnlyList<QuestionResponse>> AssignQuestionsAsync(
        long quizId, AssignQuestionsRequest request, CancellationToken ct = default)
    {
        await EnsureQuizExistsAsync(quizId, ct);

        foreach (var questionId in request.QuestionIds.Distinct())
        {
            if (!await db.Questions.AnyAsync(x => x.Id == questionId, ct))
            {
                throw new ArgumentException($"Question not found: {questionId}");
            }

            var existing = await db.QuizQuestions.IgnoreQueryFilters()
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
        return await ListQuestionsAsync(quizId, ct);
    }

    public async Task RemoveQuestionAsync(long quizId, long questionId, CancellationToken ct = default)
    {
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
        new(quiz.Id, quiz.LessonId, quiz.Title, quiz.DurationMinutes, quiz.Type, quiz.IsActive, quiz.IsRepeat, quiz.SortOrder, quiz.Questions.Count);
}
