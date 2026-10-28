using Edutor.Core.Common.Sorting;
using Edutor.Core.Common.Bulk;
using Edutor.Core.Common.Pagination;
using Edutor.Core.Common.Responses;
using Edutor.Core.Common.Storage;
using Edutor.Core.Data;
using Edutor.Core.Modules.Certificate.Abstractions;
using Edutor.Core.Modules.Certificate.Requests;
using Edutor.Core.Modules.Certificate.Responses;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.Certificate.Services;

using Certificate = Edutor.Core.Modules.Certificate.Entities.Certificate;
using Edutor.Core.Common.Queries;

public sealed class CertificateService(AppDbContext db, IFileStorage storage, ITenantFolder folders) : ICertificateService
{
    private async Task<string> FolderAsync(CancellationToken ct)
        => $"{await folders.ResolveAsync(ct: ct)}/certificates";

    public async Task<PagedResponse<CertificateResponse>> ListAsync(CertificateListRequest request, CancellationToken ct = default)
    {

        var query = db.Certificates.AsNoTracking();

        query = query.ApplyTrashFilter(request.IsDeleted);

        query = query.SearchBy(request.Search, x => x.Title);

        return await query
            .ApplyListSort(request, x => x.Title)
        .ToPagedAsync(request, ToResponse, ct);
    }

    public async Task<CertificateResponse> GetAsync(long id, CancellationToken ct = default)
    {
        var certificate = await db.Certificates
            .IgnoreQueryFilters(new[] { "SoftDelete" })
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Certificate not found: {id}");

        return ToResponse(certificate);
    }

    public async Task<CertificateResponse> CreateAsync(CertificateUpsertRequest request, CancellationToken ct = default)
    {
        var signature = request.Signature is not null
            ? await storage.UploadAsync(request.Signature, await FolderAsync(ct), ct)
            : null;

        var certificate = new Certificate
        {
            Title = request.Title,
            TemplateKey = request.TemplateKey,
            BodyText = request.BodyText,
            Signature = signature
        };

        db.Certificates.Add(certificate);
        await db.SaveChangesAsync(ct);

        return ToResponse(certificate);
    }

    public async Task<CertificateResponse> UpdateAsync(long id, CertificateUpsertRequest request, CancellationToken ct = default)
    {
        var certificate = await db.Certificates
            .IgnoreQueryFilters(new[] { "SoftDelete" })
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Certificate not found: {id}");

        if (request.RemoveSignature)
        {
            await storage.DeleteAsync(certificate.Signature, ct);
            certificate.Signature = null;
        }
        else if (request.Signature is not null)
        {
            await storage.DeleteAsync(certificate.Signature, ct);
            certificate.Signature = await storage.UploadAsync(request.Signature, await FolderAsync(ct), ct);
        }

        certificate.Title = request.Title;
        certificate.TemplateKey = request.TemplateKey;
        certificate.BodyText = request.BodyText;

        await db.SaveChangesAsync(ct);

        return ToResponse(certificate);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var certificate = await db.Certificates.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Certificate not found: {id}");

        certificate.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public Task BulkAsync(BulkRequest request, CancellationToken ct = default)
        => BulkHelper.ApplyAsync<Certificate>(db, request, ct);

    private CertificateResponse ToResponse(Certificate c) =>
        new(
            c.Id,
            c.Title,
            c.TemplateKey,
            CertificateTemplates.NameOf(c.TemplateKey),
            c.BodyText,
            storage.GetUrl(c.Signature),
            c.CreatedAt);
}
