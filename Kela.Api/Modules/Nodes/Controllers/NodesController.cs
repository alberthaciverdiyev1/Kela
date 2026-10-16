using Kela.Api.Common.Controllers;
using Kela.Api.Common.Responses;
using Kela.Api.Modules.Nodes.Auth;
using Kela.Core.Common.Storage;
using Kela.Core.Data;
using Kela.Core.Modules.Nodes;
using Kela.Core.Modules.Nodes.Abstractions;
using Kela.Nodes.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Kela.Api.Modules.Nodes.Controllers;

using WorkerNode = Kela.Core.Modules.Nodes.Entities.WorkerNode;

/// <summary>
/// Konversiya worker-ları üçün uclar. Bu uclar JWT işlətmir — cihaz öz
/// "X-Node-Token" başlığı ilə autentifikasiya olunur və yalnız öz tapşırıqlarını görür.
/// </summary>
[Route("api/nodes")]
public sealed class NodesController(
    INodeService nodes,
    IVideoJobScheduler scheduler,
    IVideoJobLifecycle lifecycle,
    IFileStorage storage,
    AppDbContext db,
    IOptions<NodeOptions> options) : ApiControllerBase
{
    private NodeOptions Settings => options.Value;

    private WorkerNode Node => HttpContext.WorkerNode();

    // ── Cihaz ────────────────────────────────────────────────────────────────

    /// <summary>Birdəfəlik qeydiyyat kodu ilə cihazı qeydə alır və access token verir.</summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<object?>>> Register(
        [FromBody] NodeRegisterRequest request, CancellationToken ct)
    {
        try
        {
            return Success<object?>(await nodes.RegisterAsync(request, ct));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Failure(ex.Message, 401);
        }
        catch (InvalidOperationException ex)
        {
            return Failure(ex.Message, 409);
        }
        catch (ArgumentException ex)
        {
            return Failure(ex.Message, 400);
        }
    }

    [Authorize(AuthenticationSchemes = NodeTokenAuthenticationOptions.Scheme)]
    [HttpPost("heartbeat")]
    public async Task<ActionResult<ApiResponse<NodeHeartbeatResponse>>> Heartbeat(
        [FromBody] NodeHeartbeatRequest request, CancellationToken ct)
        => Success(await nodes.HeartbeatAsync(Node, request, ct));

    /// <summary>Token rotasiyası — köhnə token dərhal etibarsız olur.</summary>
    [Authorize(AuthenticationSchemes = NodeTokenAuthenticationOptions.Scheme)]
    [HttpPost("token/rotate")]
    public async Task<ActionResult<ApiResponse<NodeRotateTokenResponse>>> Rotate(CancellationToken ct)
        => Success(await nodes.RotateTokenAsync(Node, ct));

    // ── Tapşırıqlar ──────────────────────────────────────────────────────────

    [Authorize(AuthenticationSchemes = NodeTokenAuthenticationOptions.Scheme)]
    [HttpPost("jobs/claim")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<JobAssignment>>>> Claim(
        [FromBody] JobClaimRequest request, CancellationToken ct)
    {
        var max = Math.Clamp(request.Max <= 0 ? 1 : request.Max, 1, 8);
        return Success(await scheduler.ClaimAsync(Node, max, ct));
    }

    [Authorize(AuthenticationSchemes = NodeTokenAuthenticationOptions.Scheme)]
    [HttpPost("jobs/{id:long}/progress")]
    public async Task<ActionResult<ApiResponse<object?>>> Progress(
        long id, [FromBody] JobProgressRequest request, CancellationToken ct)
        => Map(await lifecycle.ReportProgressAsync(Node, id, request, ct));

    [Authorize(AuthenticationSchemes = NodeTokenAuthenticationOptions.Scheme)]
    [HttpPost("jobs/{id:long}/uploads")]
    public async Task<ActionResult<ApiResponse<object?>>> Uploads(
        long id, [FromBody] JobUploadUrlRequest request, CancellationToken ct)
    {
        var (result, urls) = await lifecycle.CreateUploadUrlsAsync(Node, id, request, ct);

        if (result.IsOk) return Success<object?>(urls);

        return result.Status switch
        {
            JobOperationStatus.NotFound => Failure("Job not found", 404),
            JobOperationStatus.Conflict => Failure(result.Error ?? "Conflict", 409),
            _ => Failure(result.Error ?? "Validation failed", 422)
        };
    }

    [Authorize(AuthenticationSchemes = NodeTokenAuthenticationOptions.Scheme)]
    [HttpPost("jobs/{id:long}/complete")]
    public async Task<ActionResult<ApiResponse<object?>>> Complete(
        long id, [FromBody] JobCompleteRequest request, CancellationToken ct)
        => Map(await lifecycle.CompleteAsync(Node, id, request, ct));

    [Authorize(AuthenticationSchemes = NodeTokenAuthenticationOptions.Scheme)]
    [HttpPost("jobs/{id:long}/fail")]
    public async Task<ActionResult<ApiResponse<object?>>> Fail(
        long id, [FromBody] JobFailRequest request, CancellationToken ct)
        => Map(await lifecycle.FailAsync(Node, id, request, ct));

    /// <summary>
    /// Presigned PUT dəstəklənməyəndə (lokal storage) worker çıxışları bu ucdan keçirərək yazır.
    /// Açar yalnız tapşırığın öz qovluğunda ola bilər — başqa yerə yazmaq mümkün deyil.
    /// </summary>
    [Authorize(AuthenticationSchemes = NodeTokenAuthenticationOptions.Scheme)]
    [HttpPut("jobs/{id:long}/object")]
    [RequestSizeLimit(2L * 1024 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<object?>>> Upload(
        long id, [FromQuery] string key, [FromQuery] string token, CancellationToken ct)
    {
        var job = await db.VideoJobs.IgnoreQueryFilters()
            .FirstOrDefaultAsync(j => j.Id == id && j.WorkerNodeId == Node.Id && j.DeletedAt == null, ct);

        if (job is null) return Failure("Job not found", 404);
        if (job.AttemptToken != token) return Failure("Attempt is stale", 409);
        if (JobStatus.IsTerminal(job.Status)) return Failure("Job already finished", 409);

        var fullKey = NodePaths.Combine(job.OutputPrefix, key);
        if (fullKey is null || !NodePaths.IsInsidePrefix(job.OutputPrefix, fullKey))
        {
            return Failure("Key outside the job's output folder", 400);
        }

        var relative = key.Replace('\\', '/').TrimStart('/');
        if (relative != "thumb.jpg" && !relative.StartsWith($"{job.Resolution}/", StringComparison.Ordinal))
        {
            return Failure("Key outside the job's variant folder", 400);
        }

        await storage.UploadStreamAsync(fullKey, Request.Body, Request.ContentType ?? "application/octet-stream", ct);
        return Success<object?>(null);
    }

    private ActionResult<ApiResponse<object?>> Map(JobOperationResult result) => result.Status switch
    {
        JobOperationStatus.Ok => Success<object?>(null),
        JobOperationStatus.NotFound => Failure("Job not found", 404),
        JobOperationStatus.Conflict => Failure(result.Error ?? "Conflict", 409),
        _ => Failure(result.Error ?? "Validation failed", 422)
    };
}
