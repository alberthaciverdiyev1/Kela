using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Feedback.Entities;

/// <summary>Şagird/müəllim tərəfindən göndərilən geri bildiriş (admin panelində inbox-a düşür).</summary>
public class Feedback : BaseEntity
{
    public long? UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string UserEmail { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? Image { get; set; }

    /// <summary>new | read</summary>
    public string Status { get; set; } = FeedbackStatus.New;
}

public static class FeedbackStatus
{
    public const string New = "new";
    public const string Read = "read";
}
