namespace Kela.Core.Modules.Group.Responses;

/// <summary>Group — siyahıda başlıq altında qruplaşdırmaq üçün (məs. imtahanın dərsi).</summary>
public record OptionItem(long Id, string Text, string? Group = null);

public record GroupOptionsResponse(
    IReadOnlyList<OptionItem> Students,
    IReadOnlyList<OptionItem> Subjects,
    IReadOnlyList<OptionItem> Quizzes);
