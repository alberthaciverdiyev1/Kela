using LanguageLMS.Core.Common.Bulk;
using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Common.Storage;
using LanguageLMS.Core.Data;
using LanguageLMS.Core.Modules.Certificate.Abstractions;
using LanguageLMS.Core.Modules.Certificate.Requests;
using LanguageLMS.Core.Modules.Certificate.Responses;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Modules.Certificate.Services;

using Certificate = LanguageLMS.Core.Modules.Certificate.Entities.Certificate;

public sealed class CertificateService(AppDbContext db, IFileStorage storage) : ICertificateService
{
    private const string SignatureFolder = "certificate";

    public async Task<PagedResponse<CertificateResponse>> ListAsync(CertificateListRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var perPage = request.PerPage is < 1 or > 200 ? 20 : request.PerPage;

        var query = db.Certificates.AsNoTracking();

        if (request.IsDeleted == 1)
        {
            query = query.IgnoreQueryFilters().Where(x => x.DeletedAt != null);
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
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Certificate not found: {id}");

        return ToResponse(certificate);
    }

    public async Task<CertificateResponse> CreateAsync(CertificateUpsertRequest request, CancellationToken ct = default)
    {
        var signature = request.Signature is not null
            ? await storage.UploadAsync(request.Signature, SignatureFolder, ct)
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
            .IgnoreQueryFilters()
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
            certificate.Signature = await storage.UploadAsync(request.Signature, SignatureFolder, ct);
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
