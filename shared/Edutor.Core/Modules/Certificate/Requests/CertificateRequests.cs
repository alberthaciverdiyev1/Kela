using Edutor.Core.Common.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Core.Modules.Certificate.Requests;

public class CertificateListRequest : ListRequestBase
{
    public string? Search { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }
}

public class CertificateUpsertRequest
{
    public string Title { get; set; } = string.Empty;

    public string TemplateKey { get; set; } = string.Empty;

    public string BodyText { get; set; } = string.Empty;

    public IFormFile? Signature { get; set; }

    public bool RemoveSignature { get; set; }
}
