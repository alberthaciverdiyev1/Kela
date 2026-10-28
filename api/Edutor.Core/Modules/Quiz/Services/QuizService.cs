using Edutor.Core.Common.Pagination;
using Edutor.Core.Common.Responses;
using Edutor.Core.Common.Bulk;
using Edutor.Core.Common.Exceptions;
using Edutor.Core.Common.Sorting;
using Edutor.Core.Data;
using Edutor.Core.Common.Storage;
using Edutor.Core.Modules.Question.Responses;
using Edutor.Core.Modules.Quiz.Requests;
using Edutor.Core.Modules.Quiz.Responses;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.Quiz.Services;

using Quiz = Edutor.Core.Modules.Quiz.Entities.Quiz;
using QuizQuestion = Edutor.Core.Modules.Quiz.Entities.QuizQuestion;
using GroupQuiz = Edutor.Core.Modules.Group.Entities.GroupQuiz;
using Edutor.Core.Common.Queries;
using Edutor.Core.Common.Authorization;

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

    public async Task<IReadOnlyList<QuizGroupResponse>> ListGroupsAsync(
        long quizId, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureQuizAccessibleAsync(quizId, ownerId, ct);

        return await (from gq in db.GroupQuizzes.AsNoTracking()
                      join g in db.Groups.AsNoTracking() on gq.GroupId equals g.Id
                      where gq.QuizId == quizId
                      orderby g.Name
                      select new QuizGroupResponse(g.Id, g.Name))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<QuizGroupResponse>> ListGroupOptionsAsync(
        long quizId, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureQuizAccessibleAsync(quizId, ownerId, ct);
        var subjectId = await QuizSubjectIdAsync(quizId, ct);

        var query = db.Groups.AsNoTracking();

        if (ownerId is long id) query = query.Where(x => x.UserId == id);

        // Fənnə bağlı imtahan yalnız həmin fənni olan qruplara təklif olunur.
        if (subjectId is long sid)
        {
            query = query.Where(g => db.GroupSubjects.Any(gs => gs.GroupId == g.Id && gs.SubjectId == sid));
        }

        return await query.OrderBy(g => g.Name)
            .Select(g => new QuizGroupResponse(g.Id, g.Name))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<QuizGroupResponse>> AssignGroupsAsync(
        long quizId, AssignGroupsRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureQuizOwnedAsync(quizId, ownerId, ct);

        var ids = request.GroupIds.Distinct().ToList();
        var subjectId = await QuizSubjectIdAsync(quizId, ct);

        if (ids.Count > 0)
        {
            var groups = await db.Groups.AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new { x.Id, x.UserId })
                .ToListAsync(ct);

            if (groups.Count != ids.Count) throw new ArgumentException("Group not found");

            // Fənnə bağlı imtahan yalnız həmin fənni olan qrupa bağlana bilər.
            if (subjectId is long sid)
            {
                var withSubject = await db.GroupSubjects.AsNoTracking()
                    .Where(gs => ids.Contains(gs.GroupId) && gs.SubjectId == sid)
                    .Select(gs => gs.GroupId)
                    .Distinct()
                    .ToListAsync(ct);

                if (ids.Except(withSubject).Any())
                {
                    throw new ArgumentException("The group does not have the quiz's subject");
                }
            }

            // Qrup tərəfindəki AssignQuizzes ilə eyni qayda: teacher yalnız öz qrupuna bağlayır.
            foreach (var group in groups)
            {
                OwnershipGuard.EnsureOwned(ownerId, group.UserId, "You can only modify your own groups");
            }
        }

        foreach (var groupId in ids)
        {
            var existing = await db.GroupQuizzes.IgnoreQueryFilters(new[] { "SoftDelete" })
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
        return await ListGroupsAsync(quizId, ownerId, ct);
    }

    public async Task RemoveGroupAsync(long quizId, long groupId, long? ownerId = null, CancellationToken ct = default)
    {
        await EnsureQuizOwnedAsync(quizId, ownerId, ct);

        var group = await db.Groups.AsNoTracking()
            .Where(x => x.Id == groupId)
            .Select(x => new { x.UserId })
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException($"Group not found: {groupId}");

        OwnershipGuard.EnsureOwned(ownerId, group.UserId, "You can only modify your own groups");

        var link = await db.GroupQuizzes
            .FirstOrDefaultAsync(x => x.GroupId == groupId && x.QuizId == quizId, ct)
            ?? throw new KeyNotFoundException($"Quiz {quizId} is not assigned to group {groupId}");

        link.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    /// <summary>İmtahanın dərsinin fənni; ümumi imtahanda null.</summary>
    private async Task<long?> QuizSubjectIdAsync(long quizId, CancellationToken ct)
    {
        var lessonId = await db.Quizzes.AsNoTracking()
            .Where(x => x.Id == quizId)
            .Select(x => x.LessonId)
            .FirstOrDefaultAsync(ct);

        if (lessonId is not long id) return null;

        return await db.Lessons.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => (long?)x.SubjectId)
            .FirstOrDefaultAsync(ct);
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
