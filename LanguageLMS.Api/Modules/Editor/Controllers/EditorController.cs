using LanguageLMS.Api.Common.Authorization;
using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Modules.Auth;
using LanguageLMS.Core.Modules.Media.Abstractions;
using LanguageLMS.Core.Modules.Media.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Editor.Controllers;

[Authorize(Policy = AuthPolicies.Teacher)]
[Route("api/[controller]")]
public sealed class EditorController(IUploadService upload) : ApiControllerBase
{
    [HttpPost("upload")]
    public async Task<ActionResult<ApiResponse<UploadResponse>>> Upload(IFormFile file, CancellationToken ct)
        => Success(await upload.UploadAsync(file, "editor", ct), "Uploaded");
}
