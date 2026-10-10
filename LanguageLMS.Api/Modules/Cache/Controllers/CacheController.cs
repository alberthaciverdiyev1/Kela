using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Common.Caching;
using LanguageLMS.Core.Modules.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Cache.Controllers;

[Authorize(Policy = AuthPolicies.Teacher)]
[Route("api/[controller]")]
public sealed class CacheController(IAppCache cache) : ApiControllerBase
{
    [HttpPost("clear")]
    public ActionResult<ApiResponse<string>> Clear()
    {
        cache.Clear();
        return SuccessMessage("Cache cleared");
    }
}
