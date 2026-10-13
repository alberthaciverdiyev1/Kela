using LanguageLMS.Core.Data;
using LanguageLMS.Core.Modules.ScreenGuard.Abstractions;
using LanguageLMS.Core.Modules.ScreenGuard.Requests;

namespace LanguageLMS.Core.Modules.ScreenGuard.Services;

using ScreenViolation = LanguageLMS.Core.Modules.ScreenGuard.Entities.ScreenViolation;

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

    private static string? Truncate(string? value)
        => string.IsNullOrWhiteSpace(value) ? null
            : value.Length > MaxExtraLength ? value[..MaxExtraLength] : value;
}
