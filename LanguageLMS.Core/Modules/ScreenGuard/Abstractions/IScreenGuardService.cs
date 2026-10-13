using LanguageLMS.Core.Modules.ScreenGuard.Requests;

namespace LanguageLMS.Core.Modules.ScreenGuard.Abstractions;

public interface IScreenGuardService
{
    Task LogAsync(long studentId, LogScreenViolationRequest request, string? userAgent, CancellationToken ct = default);
}
