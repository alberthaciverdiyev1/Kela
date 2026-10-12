using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Layout;

public partial class BottomItem
{
    [Parameter] public string Href { get; set; } = "/";
    [Parameter] public string Label { get; set; } = "";
    [Parameter] public string IconName { get; set; } = "book";
    [Parameter] public bool MatchAll { get; set; }

    private string Current => "/" + Nav.ToBaseRelativePath(Nav.Uri).Split('?')[0].TrimEnd('/');

    private bool Active => MatchAll
        ? Current == "/"
        : Current == Href || Current.StartsWith(Href + "/");

    private string Css =>
        "flex min-w-16 flex-col items-center gap-1 text-[10px] font-semibold " +
        (Active ? "text-brand-671" : "text-sage-369");
}
