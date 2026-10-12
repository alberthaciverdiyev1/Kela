using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Modules.PrivacyPolicy.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.PrivacyPolicy.Controllers;

// App privacy policy oxuyur. Redaktə admin MVC-dədir.
[Route("api/[controller]")]
public sealed class PrivacyPolicyController(IPrivacyPolicyService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PrivacyPolicyResponse>>>> List(CancellationToken ct)
        => Success(await service.ListAsync(ct));

    [HttpGet("{type}")]
    public async Task<ActionResult<ApiResponse<PrivacyPolicyResponse>>> Get(string type, CancellationToken ct)
        => Success(await service.GetAsync(type, ct));
}
