using Edutor.Core.Common.Responses;
using Edutor.Core.Modules.Nodes.Entities;
using Edutor.Core.Modules.Nodes.Requests;
using Edutor.Core.Modules.Nodes.Responses;
using Edutor.Nodes.Shared.Contracts;

namespace Edutor.Core.Modules.Nodes.Abstractions;

/// <summary>
/// Worker cihazlarının qeydiyyatı, heartbeat-i, token idarəsi və admin əməliyyatları.
/// </summary>
public interface INodeService
{
    // ── Worker tərəfi (token ilə autentifikasiya olunur) ──────────────────────

    Task<NodeRegisterResponse> RegisterAsync(NodeRegisterRequest request, CancellationToken ct = default);

    /// <summary>Access token-i yoxlayır və cihazı qaytarır. Tapılmasa null.</summary>
    Task<WorkerNode?> AuthenticateAsync(string token, CancellationToken ct = default);

    Task<NodeHeartbeatResponse> HeartbeatAsync(WorkerNode node, NodeHeartbeatRequest request, CancellationToken ct = default);

    Task<NodeRotateTokenResponse> RotateTokenAsync(WorkerNode node, CancellationToken ct = default);

    // ── Admin tərəfi ─────────────────────────────────────────────────────────

    Task<PagedResponse<WorkerNodeResponse>> ListAsync(NodeListRequest request, CancellationToken ct = default);

    Task<WorkerNodeResponse> GetAsync(long id, CancellationToken ct = default);

    Task<WorkerNodeResponse> SetEnabledAsync(long id, bool enabled, CancellationToken ct = default);

    /// <summary>Token-i ləğv edir — cihaz dərhal bütün girişini itirir.</summary>
    Task RevokeTokenAsync(long id, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);

    Task<RegistrationCodeResponse> CreateCodeAsync(CreateRegistrationCodeRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<RegistrationCodeResponse>> ListCodesAsync(CancellationToken ct = default);

    Task DeleteCodeAsync(long id, CancellationToken ct = default);

    Task<NodeSummaryResponse> SummaryAsync(CancellationToken ct = default);

    Task<PagedResponse<VideoJobResponse>> ListJobsAsync(VideoJobListRequest request, CancellationToken ct = default);

    /// <summary>Uğursuz və ya ilişib qalmış tapşırığı yenidən növbəyə salır.</summary>
    Task RequeueJobAsync(long jobId, CancellationToken ct = default);
}
