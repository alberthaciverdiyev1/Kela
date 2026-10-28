using Edutor.Core.Data;
using Edutor.Core.Modules.ScreenGuard.Abstractions;
using Edutor.Core.Modules.ScreenGuard.Requests;
using Edutor.Core.Modules.ScreenGuard.Responses;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.ScreenGuard.Services;

using ScreenViolation = Edutor.Core.Modules.ScreenGuard.Entities.ScreenViolation;

public sealed class ScreenGuardService(AppDbContext db) : IScreenGuardService
{
    private const int MaxExtraLength = 500;

    public async Task LogAsync(long studentId, LogScreenViolationRequest request, string? userAgent, CancellationToken ct = default)
    {
        var type = (request.Type ?? string.Empty).Trim();
        if (type.Length == 0) return;
        if (type.Length > 40) type = type[..40];

        db.ScreenViolations.Add(new ScreenViolation
        {
            StudentId = studentId,
            Type = type,
            Path = Truncate(request.Path),
            UserAgent = Truncate(userAgent)
        });

        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ScreenViolationResponse>> ListAsync(long studentId, int limit = 100, CancellationToken ct = default)
    {
        var take = limit is < 1 or > 500 ? 100 : limit;

        return await db.ScreenViolations.AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Take(take)
            .Select(x => new ScreenViolationResponse(x.Id, x.Type, x.Path, x.UserAgent, x.CreatedAt))
            .ToListAsync(ct);
    }

    private static string? Truncate(string? value)
        => string.IsNullOrWhiteSpace(value) ? null
            : value.Length > MaxExtraLength ? value[..MaxExtraLength] : value;
}
