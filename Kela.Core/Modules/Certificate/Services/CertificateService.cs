using Kela.Core.Common.Bulk;
using Kela.Core.Common.Responses;
using Kela.Core.Common.Storage;
using Kela.Core.Data;
using Kela.Core.Modules.Certificate.Abstractions;
using Kela.Core.Modules.Certificate.Requests;
using Kela.Core.Modules.Certificate.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Certificate.Services;

using Certificate = Kela.Core.Modules.Certificate.Entities.Certificate;

public sealed class CertificateService(AppDbContext db, IFileStorage storage, ITenantFolder folders) : ICertificateService
{
    private async Task<string> FolderAsync(CancellationToken ct)
        => $"{await folders.ResolveAsync(ct: ct)}/certificates";

    public async Task<PagedResponse<CertificateResponse>> ListAsync(CertificateListRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var perPage = request.PerPage < 1 ? 20 : Math.Min(request.PerPage, 500);

        var query = db.Certificates.AsNoTracking();

        if (request.IsDeleted == 1)
        {
            query = query.IgnoreQueryFilters(new[] { "SoftDelete" }).Where(x => x.DeletedAt != null);
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

        return new PagedResponse<CertificateResponse>(data, page, lastPage, perPage, total);
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
