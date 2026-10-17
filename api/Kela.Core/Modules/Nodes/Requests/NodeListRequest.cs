using Microsoft.AspNetCore.Mvc;

namespace Kela.Core.Modules.Nodes.Requests;

public class NodeListRequest
{
    public string? Search { get; set; }

    public string? Status { get; set; }

    public int Page { get; set; } = 1;

    public int PerPage { get; set; } = 50;
}

public class CreateRegistrationCodeRequest
{
    public string? Note { get; set; }

    /// <summary>Kodun etibarlılıq müddəti (saat). 0 = müddətsiz.</summary>
    [FromForm(Name = "expires_hours")]
    public int ExpiresHours { get; set; } = 24;
}

public class VideoJobListRequest
{
    public string? Status { get; set; }

    [FromQuery(Name = "video_id")]
    public long? VideoId { get; set; }

    [FromQuery(Name = "node_id")]
    public long? NodeId { get; set; }

    public int Page { get; set; } = 1;

    public int PerPage { get; set; } = 50;
}
