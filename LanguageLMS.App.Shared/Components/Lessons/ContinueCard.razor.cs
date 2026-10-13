using Microsoft.AspNetCore.Components;
using LanguageLMS.App.Shared.Dtos.Progress;

namespace LanguageLMS.App.Shared.Components.Lessons;

public partial class ContinueCard
{
    [Parameter, EditorRequired] public ProgressItemDto Item { get; set; } = default!;

    private string TypeIcon => Item.NextContent?.Type switch
    {
        "video" => "play",
        "file" => "file-text",
        "voice" => "chart",
        _ => "book"
    };
}
