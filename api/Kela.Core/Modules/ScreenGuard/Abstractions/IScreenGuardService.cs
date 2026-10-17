using Kela.Core.Modules.ScreenGuard.Requests;
using Kela.Core.Modules.ScreenGuard.Responses;

namespace Kela.Core.Modules.ScreenGuard.Abstractions;

public interface IScreenGuardService
{
    Task LogAsync(long studentId, LogScreenViolationRequest request, string? userAgent, CancellationToken ct = default);

    Task<IReadOnlyList<ScreenViolationResponse>> ListAsync(long studentId, int limit = 100, CancellationToken ct = default);
}
