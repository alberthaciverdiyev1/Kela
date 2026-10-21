using Kela.Core.Common.Sorting;
using Kela.Core.Common.Pagination;
using Kela.Core.Common.Responses;
using Kela.Core.Common.Storage;
using Kela.Core.Common.Bulk;
using Kela.Core.Common.Exceptions;
using Kela.Core.Data;
using Kela.Core.Modules.Question.Requests;
using Kela.Core.Modules.Question.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Question.Services;

using Question = Kela.Core.Modules.Question.Entities.Question;
using Kela.Core.Common.Queries;
using Kela.Core.Common.Authorization;

public sealed class QuestionService(AppDbContext db, IFileStorage storage, ITenantFolder folders) : IQuestionService
{
    private async Task<string> FolderAsync(CancellationToken ct)
        => $"{await folders.ResolveAsync(ct: ct)}/questions";

    public async Task<PagedResponse<QuestionResponse>> ListAsync(QuestionListRequest request, CancellationToken ct = default)
    {

        var query = db.Questions.AsNoTracking();

        query = query.ApplyTrashFilter(request.IsDeleted);

        if (request.SubjectId.HasValue)
        {
            query = query.Where(x => x.SubjectId == request.SubjectId);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == request.UserId);
        }

        query = query.SearchBy(request.Search, x => x.Title!);

        return await query
            .ApplyListSort(request)
        .ToPagedAsync(request, ToResponse, ct);
    }

    public async Task<QuestionResponse> GetAsync(long id, long? ownerId = null, CancellationToken ct = default)
    {
        var question = await db.Questions.IgnoreQueryFilters(new[] { "SoftDelete" }).AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Question not found: {id}");

        OwnershipGuard.EnsureReadable(ownerId, question.UserId, question.UserId == null, "You do not have access to this question");
        return ToResponse(question);
    }

    public async Task<QuestionResponse> CreateAsync(QuestionUpsertRequest request, CancellationToken ct = default)
    {
        await EnsureSubjectExistsAsync(request.SubjectId, ct);
        if (request.UserId is long ownerId) await EnsureSubjectAccessibleAsync(request.SubjectId, ownerId, ct);

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
            VariantType = request.VariantType,
            UserId = request.UserId
        };

        await ApplyImagesAsync(question, request, ct);

        db.Questions.Add(question);
        await db.SaveChangesAsync(ct);

        return ToResponse(question);
    }

    public async Task<QuestionResponse> UpdateAsync(long id, QuestionUpsertRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        var question = await db.Questions.IgnoreQueryFilters(new[] { "SoftDelete" })
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Question not found: {id}");

        OwnershipGuard.EnsureOwned(ownerId, question.UserId, "You can only modify your own question");
        await EnsureSubjectExistsAsync(request.SubjectId, ct);
        if (ownerId.HasValue) await EnsureSubjectAccessibleAsync(request.SubjectId, ownerId.Value, ct);

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

    public async Task DeleteAsync(long id, long? ownerId = null, CancellationToken ct = default)
    {
        var question = await db.Questions.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Question not found: {id}");

        OwnershipGuard.EnsureOwned(ownerId, question.UserId, "You can only modify your own question");

        question.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task ApplyImagesAsync(Question question, QuestionUpsertRequest request, CancellationToken ct)
    {
        if (request.TitleImage is not null)
        {
            question.Title = await storage.UploadAsync(request.TitleImage, await FolderAsync(ct), ct);
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
        return await storage.UploadAsync(file, await FolderAsync(ct), ct);
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

    public Task BulkAsync(BulkRequest request, long? ownerId = null, CancellationToken ct = default)
        => db.BulkOwnedAsync<Question>(request, ownerId, x => x.UserId,
            "You can only modify your own questions", ct);

    private async Task EnsureSubjectAccessibleAsync(long subjectId, long ownerId, CancellationToken ct)
    {
        var subject = await db.Subjects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == subjectId, ct);
        if (subject is null)
        {
            throw new ForbiddenException("You do not have access to this subject");
        }

        OwnershipGuard.EnsureReadable(ownerId, subject.UserId, subject.UserId == null,
            "You do not have access to this subject");
    }

}
