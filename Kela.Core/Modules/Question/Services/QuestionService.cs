using Kela.Core.Common.Responses;
using Kela.Core.Common.Storage;
using Kela.Core.Common.Bulk;
using Kela.Core.Data;
using Kela.Core.Modules.Question.Requests;
using Kela.Core.Modules.Question.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Question.Services;

using Question = Kela.Core.Modules.Question.Entities.Question;

public sealed class QuestionService(AppDbContext db, IFileStorage storage) : IQuestionService
{
    private const string ImageFolder = "question";

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
            query = query.Where(x => EF.Functions.ILike(x.Title!, $"%{search}%"));
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
            Title = request.Title ?? string.Empty,
            VariantA = request.VariantA,
            VariantB = request.VariantB,
            VariantC = request.VariantC,
            VariantD = request.VariantD,
            VariantE = request.VariantE,
            CorrectAnswer = request.CorrectAnswer.ToUpperInvariant(),
            TitleType = request.TitleType,
            VariantType = request.VariantType
        };

        await ApplyImagesAsync(question, request, ct);

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
        question.Title = request.Title ?? string.Empty;
        question.VariantA = request.VariantA;
        question.VariantB = request.VariantB;
        question.VariantC = request.VariantC;
        question.VariantD = request.VariantD;
        question.VariantE = request.VariantE;
        question.CorrectAnswer = request.CorrectAnswer.ToUpperInvariant();
        question.TitleType = request.TitleType;
        question.VariantType = request.VariantType;

        await ApplyImagesAsync(question, request, ct);

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

    private async Task ApplyImagesAsync(Question question, QuestionUpsertRequest request, CancellationToken ct)
    {
        if (request.TitleImage is not null)
        {
            question.Title = await storage.UploadAsync(request.TitleImage, ImageFolder, ct);
            question.TitleType = "image";
        }

        if (request.VariantAImage is not null) question.VariantA = await UploadVariantAsync(question, request.VariantAImage, ct);
        if (request.VariantBImage is not null) question.VariantB = await UploadVariantAsync(question, request.VariantBImage, ct);
        if (request.VariantCImage is not null) question.VariantC = await UploadVariantAsync(question, request.VariantCImage, ct);
        if (request.VariantDImage is not null) question.VariantD = await UploadVariantAsync(question, request.VariantDImage, ct);
        if (request.VariantEImage is not null) question.VariantE = await UploadVariantAsync(question, request.VariantEImage, ct);
    }

    private async Task<string> UploadVariantAsync(Question question, IFormFile file, CancellationToken ct)
    {
        question.VariantType = "image";
        return await storage.UploadAsync(file, ImageFolder, ct);
    }

    private async Task EnsureSubjectExistsAsync(long subjectId, CancellationToken ct)
    {
        if (!await db.Subjects.AnyAsync(x => x.Id == subjectId, ct))
        {
            throw new ArgumentException($"Subject not found: {subjectId}");
        }
    }

    private QuestionResponse ToResponse(Question q) =>
        new(
            q.Id,
            q.SubjectId,
            q.Title,
            q.TitleType,
            ResolveUrl(q.TitleType, q.Title),
            q.VariantA,
            ResolveUrl(q.VariantType, q.VariantA),
            q.VariantB,
            ResolveUrl(q.VariantType, q.VariantB),
            q.VariantC,
            ResolveUrl(q.VariantType, q.VariantC),
            q.VariantD,
            ResolveUrl(q.VariantType, q.VariantD),
            q.VariantE,
            ResolveUrl(q.VariantType, q.VariantE),
            q.CorrectAnswer,
            q.VariantType);

    private string? ResolveUrl(string type, string? value)
        => type == "image" && !string.IsNullOrWhiteSpace(value) ? storage.GetUrl(value) : null;

    public Task BulkAsync(BulkRequest request, CancellationToken ct = default)
        => BulkHelper.ApplyAsync<Question>(db, request, ct);
}
