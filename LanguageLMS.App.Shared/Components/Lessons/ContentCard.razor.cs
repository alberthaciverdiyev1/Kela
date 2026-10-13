using Microsoft.AspNetCore.Components;
using LanguageLMS.App.Shared.Dtos;

namespace LanguageLMS.App.Shared.Components.Lessons;

public partial class ContentCard
{
    [Parameter, EditorRequired] public ContentItemDto Content { get; set; } = default!;
    [Parameter] public long LessonId { get; set; }

    private string IconName => Content.Type switch
    {
        "video" => "play",
        "file" => "file",
        "voice" => "chart",
        _ => "book"
    };
}
