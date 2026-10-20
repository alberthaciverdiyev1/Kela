using Kela.Core.Modules.Feedback.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Kela.Core.Common.Requests;

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

public class AddFeedbackMessageRequest
{
    public string Message { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }

    /// <summary>Göndərənin rolu — "user" | "admin" (API claim-dən doldurur).</summary>
    public string SenderRole { get; set; } = FeedbackRoles.User;

    public string SenderName { get; set; } = string.Empty;
}

public class FeedbackListRequest : ListRequestBase
{
    /// <summary>Yalnız oxunmamış mesajı olan söhbətlər.</summary>
    [FromQuery(Name = "only_unread")]
    public bool OnlyUnread { get; set; }

    public long? UserId { get; set; }
}

public class ReadFeedbackRequest
{
    /// <summary>Oxuyan tərəf: "user" | "admin".</summary>
    public string ReaderRole { get; set; } = FeedbackRoles.Admin;
}
