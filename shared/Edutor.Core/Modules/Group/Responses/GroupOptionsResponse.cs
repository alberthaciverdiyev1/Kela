namespace Edutor.Core.Modules.Group.Responses;

/// <summary>Group — siyahıda başlıq altında qruplaşdırmaq üçün (məs. imtahanın dərsi).</summary>
/// <summary>Group — siyahıda başlıq altında qruplaşdırma (məs. imtahanın dərsi).
/// SubjectId — imtahanı fənnə görə filtrləmək üçün.</summary>
public record OptionItem(long Id, string Text, string? Group = null, long? SubjectId = null);

public record GroupOptionsResponse(
    IReadOnlyList<OptionItem> Students,
    IReadOnlyList<OptionItem> Subjects,
    IReadOnlyList<OptionItem> Quizzes);
