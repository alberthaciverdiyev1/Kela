using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Core.Modules.Feedback.Requests;

public class CreateFeedbackRequest
{
    public string Message { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }

    public long? UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string UserEmail { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}

public class FeedbackListRequest
{
    public int Page { get; set; } = 1;

    public int PerPage { get; set; } = 20;

    [FromQuery(Name = "only_new")]
    public bool OnlyNew { get; set; }
}
