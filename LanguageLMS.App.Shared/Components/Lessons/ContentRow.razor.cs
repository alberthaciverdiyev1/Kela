using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Components.Lessons;

public partial class ContentRow
{
    [Parameter, EditorRequired] public ContentItemDto Content { get; set; } = default!;
    [Parameter] public bool Done { get; set; }
    [Parameter] public bool ShowOpen { get; set; }
    [Parameter] public EventCallback OnOpen { get; set; }
    [Parameter] public EventCallback OnMark { get; set; }

    private string IconName => Content.Type switch
    {
        "video" => "play",
        "file" => "file",
        "voice" => "chart",
        _ => "book"
    };
}
