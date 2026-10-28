using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.Feedback.Entities;

/// <summary>Geri bildiriş söhbəti (thread): şagird/müəllim ↔ superadmin.</summary>
public class Feedback : BaseEntity
{
    public long? UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string UserEmail { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public DateTimeOffset LastMessageAt { get; set; }

    public ICollection<FeedbackMessage> Messages { get; set; } = new List<FeedbackMessage>();
}

/// <summary>Söhbətdəki bir mesaj. SenderRole: "user" | "admin".</summary>
public class FeedbackMessage : BaseEntity
{
    public long FeedbackId { get; set; }

    public string SenderRole { get; set; } = FeedbackRoles.User;

    public string SenderName { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string? Image { get; set; }

    /// <summary>Qarşı tərəf oxuyubmu.</summary>
    public bool IsRead { get; set; }
}

public static class FeedbackRoles
{
    public const string User = "user";
    public const string Admin = "admin";
}
