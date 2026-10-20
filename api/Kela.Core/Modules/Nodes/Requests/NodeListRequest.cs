using Kela.Core.Common.Requests;

using Microsoft.AspNetCore.Mvc;

namespace Kela.Core.Modules.Nodes.Requests;

public class NodeListRequest : ListRequestBase
{
    public string? Search { get; set; }

    public string? Status { get; set; }
}

public class CreateRegistrationCodeRequest
{
    public string? Note { get; set; }

    /// <summary>Kodun etibarlılıq müddəti (saat). 0 = müddətsiz.</summary>
    [FromForm(Name = "expires_hours")]
    public int ExpiresHours { get; set; } = 24;
}

public class VideoJobListRequest : ListRequestBase
{
    public string? Status { get; set; }

    [FromQuery(Name = "video_id")]
    public long? VideoId { get; set; }

    [FromQuery(Name = "node_id")]
    public long? NodeId { get; set; }
}
