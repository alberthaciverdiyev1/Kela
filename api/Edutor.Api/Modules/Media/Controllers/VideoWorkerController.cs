using System.Security.Cryptography;
using System.Text;
using Edutor.Api.Common.Controllers;
using Edutor.Api.Common.Responses;
using Edutor.Core.Common.Localization;
using Edutor.Core.Common.Storage;
using Edutor.Core.Modules.Media.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Edutor.Api.Modules.Media.Controllers;

/// <summary>
/// Xarici video konversiya worker-ı üçün daxili uclar. JWT yoxdur — worker istifadəçi
/// deyil, ona görə paylaşılan açar ("X-Worker-Key" başlığı) ilə qorunur.
/// Açar konfiqurasiyada boşdursa uclar tamamilə bağlıdır.
/// </summary>
[AllowAnonymous]
[Route("api/video")]
public sealed class VideoWorkerController(
    IVideoProcessingService service,
    IOptions<WorkerOptions> worker,
    IStringLocalizer<SharedResource> L) : ApiControllerBase
{
    private const string Header = "X-Worker-Key";

    /// <summary>Worker işə başlayanda — video "processing" olur.</summary>
    [HttpPost("processing")]
    public async Task<ActionResult<ApiResponse<object?>>> Processing([FromBody] VideoProcessingRequest request, CancellationToken ct)
    {
        if (!Authorized()) return Failure(L["Api_WorkerUnauthorized"], 401);

        return await service.MarkProcessingAsync(request.SourcePath, ct)
            ? Success<object?>("processing")
            : Failure(L["Api_VideoNotFound"], 404);
    }

    /// <summary>
    /// Konversiya bitəndə — m3u8 + thumbnail qeydə alınır. Video hələ tapılmırsa 404
    /// qaytarılır ki, kuyruk təkrar yoxlasın (R2 event-i DB sətrindən əvvəl gələ bilər).
    /// </summary>
    [HttpPost("processed")]
    public async Task<ActionResult<ApiResponse<object?>>> Processed([FromBody] VideoProcessedRequest request, CancellationToken ct)
    {
        if (!Authorized()) return Failure(L["Api_WorkerUnauthorized"], 401);

        return await service.MarkProcessedAsync(request, ct)
            ? Success<object?>("ok")
            : Failure(L["Api_VideoNotFound"], 404);
    }

    private bool Authorized()
    {
        var expected = worker.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(expected)) return false;

        var provided = Request.Headers[Header].ToString();
        if (string.IsNullOrEmpty(provided)) return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(provided),
            Encoding.UTF8.GetBytes(expected));
    }
}
