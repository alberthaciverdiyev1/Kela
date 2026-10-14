namespace Kela.Core.Modules.Group.Responses;

public record OptionItem(long Id, string Text);

public record GroupOptionsResponse(
    IReadOnlyList<OptionItem> Students,
    IReadOnlyList<OptionItem> Subjects,
    IReadOnlyList<OptionItem> Quizzes);
