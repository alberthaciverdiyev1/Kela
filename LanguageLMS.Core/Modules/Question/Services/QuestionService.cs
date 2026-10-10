using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Data;
using LanguageLMS.Core.Modules.Question.Requests;
using LanguageLMS.Core.Modules.Question.Responses;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Modules.Question.Services;

using Question = LanguageLMS.Core.Modules.Question.Entities.Question;

public sealed class QuestionService(AppDbContext db) : IQuestionService
{
    public async Task<PagedResponse<QuestionResponse>> ListAsync(QuestionListRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var perPage = request.PerPage is < 1 or > 200 ? 20 : request.PerPage;

        var query = db.Questions.AsNoTracking();

        if (request.IsDeleted == 1)
        {
            query = query.IgnoreQueryFilters().Where(x => x.DeletedAt != null);
        }

        if (request.SubjectId.HasValue)
        {
            query = query.Where(x => x.SubjectId == request.SubjectId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => EF.Functions.ILike(x.Title, $"%{search}%"));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(ct);

        var data = items.Select(ToResponse).ToList();
        var lastPage = (int)Math.Ceiling(total / (double)perPage);

        return new PagedResponse<QuestionResponse>(data, page, lastPage, perPage, total);
    }

    public async Task<QuestionResponse> GetAsync(long id, CancellationToken ct = default)
    {
        var question = await db.Questions.IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Question not found: {id}");

        return ToResponse(question);
    }

    public async Task<QuestionResponse> CreateAsync(QuestionUpsertRequest request, CancellationToken ct = default)
    {
        await EnsureSubjectExistsAsync(request.SubjectId, ct);

        var question = new Question
        {
            SubjectId = request.SubjectId,
            Title = request.Title,
            VariantA = request.VariantA,
            VariantB = request.VariantB,
            VariantC = request.VariantC,
            VariantD = request.VariantD,
            VariantE = request.VariantE,
            CorrectAnswer = request.CorrectAnswer.ToUpperInvariant(),
            VariantType = request.VariantType
        };

        db.Questions.Add(question);
        await db.SaveChangesAsync(ct);

        return ToResponse(question);
    }

    public async Task<QuestionResponse> UpdateAsync(long id, QuestionUpsertRequest request, CancellationToken ct = default)
    {
        var question = await db.Questions.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Question not found: {id}");

        await EnsureSubjectExistsAsync(request.SubjectId, ct);

        question.SubjectId = request.SubjectId;
        question.Title = request.Title;
        question.VariantA = request.VariantA;
        question.VariantB = request.VariantB;
        question.VariantC = request.VariantC;
        question.VariantD = request.VariantD;
        question.VariantE = request.VariantE;
        question.CorrectAnswer = request.CorrectAnswer.ToUpperInvariant();
        question.VariantType = request.VariantType;
        await db.SaveChangesAsync(ct);

        return ToResponse(question);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var question = await db.Questions.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Question not found: {id}");

        question.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task EnsureSubjectExistsAsync(long subjectId, CancellationToken ct)
    {
        if (!await db.Subjects.AnyAsync(x => x.Id == subjectId, ct))
        {
            throw new ArgumentException($"Subject not found: {subjectId}");
        }
    }

    private static QuestionResponse ToResponse(Question q) =>
        new(q.Id, q.SubjectId, q.Title, q.VariantA, q.VariantB, q.VariantC, q.VariantD, q.VariantE, q.CorrectAnswer, q.VariantType);
}
